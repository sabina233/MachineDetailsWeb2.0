using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using web_api2022.Data;
using web_api2022.DTOs;
using web_api2022.Models;

namespace YourProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateController : ControllerBase
    {
        private readonly IOptionsMonitor<UpdateSettings> _settingsMonitor;
        private readonly DataContext _context;
        // 静态信号量，确保跨请求的同步
        private static readonly SemaphoreSlim _manifestLock = new SemaphoreSlim(1, 1);

        public UpdateController(IOptionsMonitor<UpdateSettings> settingsMonitor, DataContext context)
        {
            _settingsMonitor = settingsMonitor;
            _context = context;
        }

        [HttpGet("manifest")]
        public async Task<IActionResult> GetManifest()
        {
            var settings = _settingsMonitor.CurrentValue;
            string latestVersion = settings.CurrentLatest;
            string releaseRoot = settings.ReleaseRoot;

            string versionPath = Path.Combine(releaseRoot, latestVersion);
            string manifestPath = Path.Combine(versionPath, "manifest.json");

            // 1. 如果文件已存在，直接返回，不走生成逻辑
            if (System.IO.File.Exists(manifestPath))
            {
                return Content(await System.IO.File.ReadAllTextAsync(manifestPath), "application/json");
            }

            // 2. 如果文件不存在，进入生成逻辑（加锁）
            await _manifestLock.WaitAsync();
            try
            {
                // 双重检查：防止在等待锁的过程中，另一个请求已经把文件生成好了
                if (System.IO.File.Exists(manifestPath))
                {
                    return Content(await System.IO.File.ReadAllTextAsync(manifestPath), "application/json");
                }

                // 3. 开始扫描并生成清单
                var manifest = GenerateManifestInternal(versionPath, latestVersion);

                // 4. 序列化并保存到磁盘
                string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(manifestPath, json);

                return Content(json, "application/json");
            }
            finally
            {
                _manifestLock.Release(); // 释放锁
            }
        }

        private UpdateManifest GenerateManifestInternal(string versionPath, string version)
        {
            string filesPath = Path.Combine(versionPath, "Files");
            if (!Directory.Exists(filesPath))
                throw new DirectoryNotFoundException($"未找到文件目录: {filesPath}");

            var manifest = new UpdateManifest
            {
                Version = version,
                Files = new List<FileItem>()
            };

            var allFiles = Directory.GetFiles(filesPath, "*.*", SearchOption.AllDirectories);

            // 性能优化：对于大量文件，可以使用 Parallel.ForEach 但要注意线程安全
            foreach (var filePath in allFiles)
            {
                string relativePath = Path.GetRelativePath(filesPath, filePath).Replace("\\", "/");

                // 过滤maximpost文件，避免站点配置的替换
                if (relativePath.EndsWith("maximpost.json", StringComparison.OrdinalIgnoreCase)||
                    relativePath.EndsWith("Matching key.key", StringComparison.OrdinalIgnoreCase)||
                    relativePath.EndsWith("Maxim Licence.key", StringComparison.OrdinalIgnoreCase))
                    continue;
                manifest.Files.Add(new FileItem
                {
                    Path = relativePath,
                    Hash = CalculateMD5(filePath)
                });
            }

            return manifest;
        }

        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }


        /// <summary>
        /// 下载差异文件
        /// GET: /api/update/download?version=1.0.2&file=MainApp.exe
        /// </summary>
        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string version, [FromQuery] string file)
        {
            var settings = _settingsMonitor.CurrentValue;

            // 安全处理路径，防止跳转到根目录之外
            string safeFile = file.Replace("..", "").Replace("/", Path.DirectorySeparatorChar.ToString());
            string filePath = Path.Combine(settings.ReleaseRoot, version, "Files", safeFile);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"文件未找到: {file}");

            // 返回文件流，适合大文件且内存友好
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // 指定 Content-Type 为二进制流，并提供下载文件名
            return File(stream, "application/octet-stream", Path.GetFileName(filePath));
        }

        /// <summary>
        /// 下载差异文件压缩包
        /// </summary>
        /// <param name="version"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        // 后端：UpdateController.cs 增加此接口
        [HttpPost("download-bundle")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> DownloadBundle([FromQuery] string version, [FromBody] List<string> files)
        {
            var settings = _settingsMonitor.CurrentValue;
            string versionPath = Path.Combine(settings.ReleaseRoot, version, "Files");

            // 1. 在内存中创建流
            var ms = new MemoryStream();

            try
            {
                // 2. 使用 using 确保 ZipArchive 在返回前彻底完成打包并关闭
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        // 兼容路径分隔符
                        string safeFile = file.Replace("/", Path.DirectorySeparatorChar.ToString());
                        string filePath = Path.Combine(versionPath, safeFile);

                        if (System.IO.File.Exists(filePath))
                        {
                            // 创建 ZIP 内部条目
                            var entry = archive.CreateEntry(file, CompressionLevel.Fastest);
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(filePath))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                } // <--- 关键点：ZipArchive 在这里 Dispose，把中央目录(索引)写进 MemoryStream

                // 3. 指针重置到开头，准备读取下载
                ms.Position = 0;

                // 4. 返回 File 结果，ASP.NET Core 会自动处理 Content-Type 和下载文件名
                return File(ms, "application/octet-stream", $"update_{version}.zip");
            }
            catch (Exception)
            {
                ms.Dispose();
                return StatusCode(500, "Internal Server Error during bundling");
            }
        }

        /// <summary>
        /// 根据用户名获取对应的 maximpost.json 配置文件
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("generate/{username}")]
        public async Task<IActionResult> GetMaximPostConfig(string username)
        {
            try
            {
                // 1. 从数据库获取该账号的配置参数
                var account = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (account == null)
                {
                    return NotFound(new { message = "未找到该账号的配置信息" });
                }

                // 2. 组装成对应的 JSON 结构
                // 组装成对应的 JSON 结构
                var config = new MaximPostResponse
                {
                    Api = account.Api,
                    PrintShopId = account.PrintShopId,
                    BrandId_default = account.BrandIdDefault,
                    ProductionMode = account.ProductionMode,

                    // 使用 Trim 结合字符数组，一次性清理所有杂质
                    BrandId = account.BrandIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().Trim('\"', '\r', '\n'))
                        .ToList(),

                    BrandName = account.BrandNames
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().Trim('\"', '\r', '\n'))
                        .ToList()
                };

                // 3. 将对象序列化并作为文件返回
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // 允许不对特殊符号和非 ASCII 字符进行转义
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 设置序列化选项以保持良好的格式（可选）

                string jsonString = JsonSerializer.Serialize(config, options);

                var fileBytes = Encoding.UTF8.GetBytes(jsonString);

                // 返回文件流，浏览器或请求端会自动识别为 maximpost.json
                return File(fileBytes, "application/json", "MaximPost.json");

            }
            catch (Exception e)
            {
                return StatusCode(0, $"Get MaximPost file error:{e.Message}!");
                throw;
            }
        }
    }
}