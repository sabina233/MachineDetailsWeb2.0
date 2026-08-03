using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_api2022.Data;
using Microsoft.AspNetCore.SignalR;
using web_api2022.Hubs;
using web_api2022.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using web_api2022.Configurations;
using Microsoft.Data.SqlClient;
using System.Globalization;
using web_api2022.Services;

namespace web_api2022.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly DataContext _context; // 数据上下文，用于与数据库交互
        private readonly IHubContext<DataHub> _hubContext; // 用于SignalR推送实时数据
        private readonly string _liandian_apiKey; // API Key，用于认证
        private readonly string _connectionString; // 数据库连接字符串
        private readonly IProductionDashboardService _dashboardService; // 生产看板聚合服务

        // 构造函数注入 DataContext 和 IHubContext 依赖
        public DataController(DataContext context, IHubContext<DataHub> hubContext, IOptions<ApiSettings> settings, IConfiguration configuration, IProductionDashboardService dashboardService)
        {
            _context = context;
            _hubContext = hubContext;
            _liandian_apiKey = settings.Value.Liandian_ApiKey;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _dashboardService = dashboardService;
        }

        // GET: /api/Data/dashboard/summary?date=yyyy-MM-dd
        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] string date)
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var selectedDate))
            {
                return BadRequest(new { message = "date must use yyyy-MM-dd" });
            }

            return Ok(await _dashboardService.GetDashboardAsync(
                selectedDate, null, HttpContext?.RequestAborted ?? CancellationToken.None));
        }

        // GET: /api/Data/dashboard/liandian?date=yyyy-MM-dd&category=roll|single
        [HttpGet("dashboard/liandian")]
        public async Task<IActionResult> GetLiandianDashboard([FromQuery] string date, [FromQuery] string? category = null)
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var selectedDate))
            {
                return BadRequest(new { message = "date must use yyyy-MM-dd" });
            }

            if (category is not (null or "roll" or "single"))
            {
                return BadRequest(new { message = "category must be roll or single" });
            }

            return Ok(await _dashboardService.GetDashboardAsync(
                selectedDate, category, HttpContext?.RequestAborted ?? CancellationToken.None));
        }

        /// <summary>
        /// 这个接口用于接收数据并将其存储在数据库中，同时会删除旧数据
        /// </summary>
        /*[Authorize]*/ // 需要JWT认证
        [HttpPost] //默认为data
        public async Task<IActionResult> State([FromBody] DataModel data)
        {
            // 验证 API Key
            if (!Request.Headers.TryGetValue("LP_KEY", out var extractedApiKey))
                return Unauthorized(new { message = "API Key is missing" });

            if (!_liandian_apiKey.Equals(extractedApiKey))
                return Unauthorized(new { message = "Invalid API Key" });



            if (ModelState.IsValid) // 验证模型是否有效
            {
                data.Timestamp = DateTime.Now; // 设置数据的时间戳

                // 使用事务保证操作的完整性
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 删除 DataLatest 中相同 MachineName 的旧数据
                    var existingLatest = _context.DataLatest.Where(d => d.MachineName == data.MachineName);
                    _context.DataLatest.RemoveRange(existingLatest);

                    // 插入新数据到 DataLatest 和 DataRecords
                    _context.DataLatest.Add(data);

                    // 将 DataModel 映射为 DataRecordModel 插入到 DataRecords
                    var dataRecord = new DataRecordModel
                    {
                        MachineName = data.MachineName,
                        Timestamp = data.Timestamp,
                        Id = data.Id,
                        Code = data.Code,
                        CountryCode = data.CountryCode,
                        Message = data.Message,
                        Data = new NestedData
                        {
                            // 根据 NestedData 的字段手动映射
                            State = data.Data.State,
                            Speed = data.Data.Speed,
                            SetSpeed = data.Data.SetSpeed,
                            TaskName = data.Data.TaskName,
                            TaskNum = data.Data.TaskNum,
                            ResidueTask = data.Data.ResidueTask,
                            CardReplace = data.Data.CardReplace,
                            FileName = data.Data.FileName,
                            TotalRecords = data.Data.TotalRecords,
                            ProductQuantity = data.Data.ProductQuantity,
                            TotalSuccess = data.Data.TotalSuccess,
                            TotalFail = data.Data.TotalFail,
                            BatchNum = data.Data.BatchNum,
                            BatchSuccess = data.Data.BatchSuccess,
                            BatchFail = data.Data.BatchFail,
                            Trigger1 = data.Data.Trigger1,
                            Trigger2 = data.Data.Trigger2,
                            Inventory1 = data.Data.Inventory1,
                            Inventory2 = data.Data.Inventory2,
                            Discard1 = data.Data.Discard1,
                            Discard2 = data.Data.Discard2,
                            RejectRate = (int)data.Data.RejectRate,
                            DataErr = data.Data.DataErr
                        }
                    };

                    // 将新记录添加到数据库
                    _context.Set<DataRecordModel>().Add(dataRecord);
                    await _context.SaveChangesAsync(); // 保存更改

                    await transaction.CommitAsync(); // 提交事务
                    return Ok(new { State = 0, message = "Data saved successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // 回滚事务
                    return StatusCode(500, new { State = 1, message = $"Error saving data: {ex.Message}" });
                }
            }
            return BadRequest(ModelState); // 如果模型无效，返回错误
        }


        /// <summary>
        /// 获取机型数据，包括名称、状态、速度、设定速度、产量
        /// </summary>
        [HttpGet("MachineStates")]
        public IActionResult GetMachineStates()
        {
            var machines = _context.DataLatest
                .Where(d => !string.IsNullOrEmpty(d.MachineName)) // 排除 MachineName 为空的数据
                .Select(d => new
                {
                    d.MachineName,

                    State = d.Data.State, // 状态字段，0 为灰色，1 为正常
                    Speed = d.Data.Speed,
                    SetSpeed = d.Data.SetSpeed,
                    ProductQuantity = d.Data.ProductQuantity
                })
                .ToList(); // 获取数据

            return Ok(machines); // 返回结果
        }

        /// <summary>
        /// 获取机型详细数据，支持根据日期范围筛选
        /// </summary>
        [HttpGet("MachineDetails")]
        public IActionResult GetMachineDetails(string machineName, string range, DateTime? date = null)
        {
            DateTime startDate, endDate;
            DateTime selectedDate = date ?? DateTime.Today; // 默认选择当天

            // 根据传入的 range 参数，计算开始和结束日期
            switch (range.ToLower())
            {
                case "day":
                    startDate = selectedDate.Date;
                    endDate = startDate.AddDays(1);
                    break;
                case "week":
                    int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = selectedDate.AddDays(-1 * diff).Date;
                    endDate = startDate.AddDays(7);
                    break;
                case "month":
                    startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
                default:
                    return BadRequest("无效的日期范围");
            }

            var machine = _context.DataLatest
                .Where(d => d.MachineName == machineName)
                .Select(d => new
                {
                    d.MachineName,
                    d.Data.TaskName,
                    History = _context.DataRecords
                        .Where(r => r.MachineName == machineName && r.Timestamp >= startDate && r.Timestamp < endDate)
                        .GroupBy(r => new { r.MachineName, r.Data.TaskName })
                        .Select(g => new
                        {
                            g.Key.MachineName,
                            g.Key.TaskName,
                            StartTime = g.Min(d => d.Timestamp),
                            EndTime = g.Max(d => d.Timestamp)
                        })
                        .OrderBy(h => h.StartTime) // 按照开始时间排序
                        .ToList(),
                    ProductQuantity = _context.DataRecords
                        .Where(r => r.MachineName == machineName)
                        .Distinct()
                        .Sum(r => r.Data.ProductQuantity)
                })
                .FirstOrDefault(); // 获取机器数据

            if (machine == null)
                return NotFound(new { message = "Machine not found" });

            return Ok(machine); // 返回结果
        }

        /// <summary>
        /// 获取实时数据
        /// </summary>
        [HttpGet("RealTimeData")]
        public IActionResult GetRealTimeData(string machineName)
        {
            try
            {
                var result = _context.DataLatest
                    .Where(d => d.MachineName == machineName)
                    .Select(d => new
                    {
                        d.MachineName,
                        d.Timestamp,
                        d.Data.TaskName,
                        d.Data.Speed,
                        d.Data.SetSpeed
                    })
                    .OrderByDescending(d => d.Timestamp) // 按照时间戳降序排序
                    .ToList();

                return Ok(result); // 返回结果
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}"); // 返回服务器错误
            }
        }

        /// <summary>
        /// 获取历史生产记录（服务端 GroupBy，避免全量拉取到内存）
        /// </summary>
        [HttpGet("HistoryRecords")]
        public async Task<IActionResult> GetHistoryRecords(string machineName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow.AddDays(1);

                var sql = @"
                    SELECT 
                        TaskName,
                        MIN(Timestamp) AS StartTime,
                        MAX(Timestamp) AS EndTime,
                        DATEDIFF(SECOND, MIN(Timestamp), MAX(Timestamp)) AS DurationSeconds,
                        ISNULL(MAX(CASE WHEN ProductQuantity > 0 THEN ProductQuantity ELSE NULL END), 0) AS ProductQuantity
                    FROM DataRecords
                    WHERE MachineName = @machineName 
                      AND Timestamp >= @startDate 
                      AND Timestamp <= @endDate
                    GROUP BY TaskName
                    ORDER BY MIN(Timestamp)";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@machineName", machineName);
                cmd.Parameters.AddWithValue("@startDate", start);
                cmd.Parameters.AddWithValue("@endDate", end);

                var result = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var startTime = reader.GetDateTime(1);
                    var endTime = reader.GetDateTime(2);
                    var durationSeconds = reader.GetInt32(3);
                    var durationHours = Math.Round(durationSeconds / 3600.0, 4);
                    var safeDurationHours = Math.Max(durationSeconds / 3600.0, 1.0 / 3600); // 最少1秒
                    var productQuantity = reader.GetInt32(4);
                    var unitsPerHour = safeDurationHours > 0
                        ? Math.Round(productQuantity / safeDurationHours, 2)
                        : 0;

                    result.Add(new
                    {
                        TaskName = reader.GetString(0),
                        StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        DurationHours = durationHours,
                        ProductQuantity = productQuantity,
                        UnitsPerHour = unitsPerHour
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }



        /// <summary>
        /// 获取订单生产图表数据（服务端 GroupBy）
        /// </summary>
        [HttpGet("OrderProductionChart")]
        public async Task<IActionResult> GetOrderProductionChart(string machineName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow.AddDays(1);

                var sql = @"
                    SELECT 
                        CONVERT(DATE, Timestamp) AS [Date],
                        COUNT(DISTINCT TaskName) AS OrderCount,
                        SUM(DISTINCT ProductQuantity) AS ProductQuantity
                    FROM DataRecords
                    WHERE MachineName = @machineName 
                      AND Timestamp >= @startDate 
                      AND Timestamp <= @endDate
                    GROUP BY CONVERT(DATE, Timestamp)
                    ORDER BY [Date]";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@machineName", machineName);
                cmd.Parameters.AddWithValue("@startDate", start);
                cmd.Parameters.AddWithValue("@endDate", end);

                var result = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        OrderCount = reader.GetInt32(1),
                        ProductQuantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定日期的生产数据汇总
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="dailydate"></param>
        /// <returns></returns>
        //    [HttpGet("BrandProductionChart")]
        //    public IActionResult GetFilenameSummary(string machineName, DateTime? startDate, DateTime? endDate)
        //    {
        //        try
        //        {
        //            startDate ??= DateTime.UtcNow.AddDays(-1); // 默认最近 1 天
        //            endDate ??= DateTime.UtcNow;
        //            // 从数据库加载数据
        //            var data = _context.DataRecords
        //                .Where(d => d.MachineName == machineName && d.Timestamp >= startDate && d.Timestamp < endDate) // 排除空值
        //                .Select(d => new
        //                {
        //                    FileName = d.Data.FileName,
        //                    ProductQuantity = d.Data.ProductQuantity
        //                })
        //                .ToList();

        //            // 截取最后一个 '\' 后的前三位字符，并对 ProductQuantity 进行汇总
        //            var result = data
        //                .GroupBy(d =>
        //                {
        //                    var fileName = d.FileName;
        //                    var lastPart = fileName.Substring(fileName.LastIndexOf("\\") + 1); // 获取最后一部分
        //                    return lastPart.Substring(0, 3); // 截取前三位
        //                })
        //                .Select(g => new
        //                {
        //                    //date = endDate.Value.ToString("yyyy-MM-dd"), // 日期
        //                    Name = g.Key, // 文件名的前三位
        //                    ProductQuantity = g.Select(d => d.ProductQuantity).Distinct().Sum()
        //                })
        //                .OrderBy(r => r.Name) // 按名称排序
        //                .ToList();

        //            return Ok(result);
        //        }
        //        catch (Exception ex)
        //        {
        //            // 记录错误日志
        //            Console.WriteLine($"Error in GetFilenameSummary API: {ex.Message}");
        //            return StatusCode(500, new { message = "服务器内部错误" });
        //        }
        //    }

        /// <summary>
        /// 获取指定日期的生产数据汇总（服务端筛选+列投影，减少数据传输量）
        /// </summary>
        [HttpGet("BrandProductionChart")]
        public async Task<IActionResult> GetFilenameSummary(string machineName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow;
                var end = endDate ?? DateTime.UtcNow.AddDays(1);

                // SQL 端做筛选和列投影，只取需要的两列
                var sql = @"
                    SELECT FileName, ProductQuantity
                    FROM DataRecords
                    WHERE MachineName = @machineName 
                      AND Timestamp >= @startDate 
                      AND Timestamp < @endDate";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@machineName", machineName);
                cmd.Parameters.AddWithValue("@startDate", start);
                cmd.Parameters.AddWithValue("@endDate", end);

                var data = new List<(string FileName, int ProductQuantity)>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    data.Add((
                        reader.IsDBNull(0) ? "" : reader.GetString(0),
                        reader.IsDBNull(1) ? 0 : reader.GetInt32(1)
                    ));
                }

                // C# 端做品牌名称提取和分组（字符串逻辑复杂，不适合放 SQL）
                var result = data
                    .GroupBy(d =>
                    {
                        var fileName = d.FileName;
                        var lastSlash = fileName.LastIndexOf("\\");
                        var lastPart = lastSlash >= 0 ? fileName.Substring(lastSlash + 1) : fileName;
                        var name = new string(lastPart.TakeWhile(char.IsLetter).ToArray());
                        return name;
                    })
                    .Select(g => new
                    {
                        Name = g.Key,
                        ProductQuantity = g.Select(d => d.ProductQuantity).Distinct().Sum()
                    })
                    .OrderBy(r => r.Name)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFilenameSummary API: {ex.Message}");
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }
    }
}
