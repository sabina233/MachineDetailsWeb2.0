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

        // GET: /api/Data/dashboard/summary?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd
        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetDashboardSummary(
            [FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (!TryParseRange(startDate, endDate, out var start, out var end))
            {
                return BadRequest(new { message = "startDate/endDate must use yyyy-MM-dd and endDate must be >= startDate" });
            }

            return Ok(await _dashboardService.GetDashboardAsync(
                start, end, null, HttpContext?.RequestAborted ?? CancellationToken.None));
        }

        // GET: /api/Data/dashboard/liandian?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd&category=roll|single
        [HttpGet("dashboard/liandian")]
        public async Task<IActionResult> GetLiandianDashboard(
            [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string? category = null)
        {
            if (!TryParseRange(startDate, endDate, out var start, out var end))
            {
                return BadRequest(new { message = "startDate/endDate must use yyyy-MM-dd and endDate must be >= startDate" });
            }

            if (category is not (null or "roll" or "single"))
            {
                return BadRequest(new { message = "category must be roll or single" });
            }

            return Ok(await _dashboardService.GetDashboardAsync(
                start, end, category, HttpContext?.RequestAborted ?? CancellationToken.None));
        }

        // GET: /api/Data/dashboard/machines
        // 返回 DataLatest 中当前所有机台（含区域/状态/当前任务），供区域分组页使用
        [HttpGet("dashboard/machines")]
        public async Task<IActionResult> GetMachines()
        {
            var machines = await _context.DataLatest
                .AsNoTracking()
                .OrderBy(d => d.MachineName)
                .Select(d => new
                {
                    d.MachineName,
                    d.CountryCode,
                    State = d.Data.State,
                    TaskName = d.Data.TaskName,
                    Speed = d.Data.Speed,
                    SetSpeed = d.Data.SetSpeed,
                    ProductQuantity = d.Data.ProductQuantity,
                    d.Timestamp
                })
                .ToListAsync();

            return Ok(machines);
        }

        private static bool TryParseRange(string startDate, string endDate, out DateOnly start, out DateOnly end)
        {
            start = default;
            end = default;
            return DateOnly.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out start)
                   && DateOnly.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out end)
                   && end >= start;
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
                        .Where(r => r.MachineName == machineName && r.Timestamp >= startDate && r.Timestamp < endDate && !string.IsNullOrEmpty(r.Data.TaskName))
                        .GroupBy(r => r.Data.TaskName)
                        .Select(g => g.Max(r => r.Data.ProductQuantity))
                        .Sum()
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
                        State = d.Data.State,
                        TaskName = d.Data.TaskName,
                        Speed = d.Data.Speed,
                        SetSpeed = d.Data.SetSpeed,
                        ProductQuantity = d.Data.ProductQuantity
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
                      AND TaskName IS NOT NULL AND TaskName <> ''
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
                    var productQuantity = reader.GetInt32(4);
                    var unitsPerHour = durationSeconds > 0
                        ? Math.Round(productQuantity / (durationSeconds / 3600.0), 2)
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
        /// 获取订单生产图表数据（服务端 GroupBy，按任务去重取最大产量后按日汇总）
        /// </summary>
        [HttpGet("OrderProductionChart")]
        public async Task<IActionResult> GetOrderProductionChart(string machineName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow.AddDays(1);

                var sql = @"
                    WITH DailyTasks AS (
                        SELECT 
                            CONVERT(DATE, Timestamp) AS [Date],
                            TaskName,
                            MAX(ProductQuantity) AS TaskQuantity
                        FROM DataRecords
                        WHERE MachineName = @machineName 
                          AND Timestamp >= @startDate 
                          AND Timestamp <= @endDate
                          AND TaskName IS NOT NULL AND TaskName <> ''
                        GROUP BY CONVERT(DATE, Timestamp), TaskName
                    )
                    SELECT 
                        [Date],
                        COUNT(TaskName) AS OrderCount,
                        ISNULL(SUM(TaskQuantity), 0) AS ProductQuantity
                    FROM DailyTasks
                    GROUP BY [Date]
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
        /// 获取指定日期的品牌生产数据汇总（按任务去重取最大产量后按品牌前缀汇总）
        /// </summary>
        [HttpGet("BrandProductionChart")]
        public async Task<IActionResult> GetFilenameSummary(string machineName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow;
                var end = endDate ?? DateTime.UtcNow.AddDays(1);

                // SQL 端先按任务聚合，取每个任务的最终 FileName 和最大 ProductQuantity
                var sql = @"
                    SELECT 
                        TaskName,
                        MAX(FileName) AS FileName,
                        MAX(ProductQuantity) AS ProductQuantity
                    FROM DataRecords
                    WHERE MachineName = @machineName 
                      AND Timestamp >= @startDate 
                      AND Timestamp < @endDate
                      AND TaskName IS NOT NULL AND TaskName <> ''
                    GROUP BY TaskName";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@machineName", machineName);
                cmd.Parameters.AddWithValue("@startDate", start);
                cmd.Parameters.AddWithValue("@endDate", end);

                var tasks = new List<(string TaskName, string FileName, int ProductQuantity)>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tasks.Add((
                        reader.IsDBNull(0) ? "" : reader.GetString(0),
                        reader.IsDBNull(1) ? "" : reader.GetString(1),
                        reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                    ));
                }

                // C# 端按每个任务提取品牌并分组汇总
                var result = tasks
                    .GroupBy(t =>
                    {
                        var fileName = t.FileName;
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var lastSlash = fileName.LastIndexOf("\\");
                            var lastPart = lastSlash >= 0 ? fileName.Substring(lastSlash + 1) : fileName;
                            var name = new string(lastPart.TakeWhile(char.IsLetter).ToArray());
                            if (!string.IsNullOrEmpty(name)) return name;
                        }
                        var taskPrefix = new string((t.TaskName ?? "").TakeWhile(char.IsLetter).ToArray());
                        return !string.IsNullOrEmpty(taskPrefix) ? taskPrefix : "OTHER";
                    })
                    .Select(g => new
                    {
                        Name = g.Key,
                        ProductQuantity = g.Sum(t => t.ProductQuantity)
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
