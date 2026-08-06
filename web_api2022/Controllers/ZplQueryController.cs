// Controllers/ZplQueryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_api2022.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System;
using web_api2022.Services;

namespace web_api2022.Controllers
{
    [ApiController]
    [Route("api/zpl")]
    public class ZplQueryController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly string _connectionString;
        private readonly IProductionDashboardService _dashboardService;
        public ZplQueryController(DataContext db, IConfiguration configuration, IProductionDashboardService dashboardService)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _dashboardService = dashboardService;
        }

        // GET: /api/zpl/dashboard/daily-summary?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd
        [HttpGet("dashboard/daily-summary")]
        public async Task<IActionResult> GetDailySummary(
            [FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (!DateOnly.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var start)
                || !DateOnly.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var end)
                || end < start)
            {
                return BadRequest(new { message = "startDate/endDate must use yyyy-MM-dd and endDate must be >= startDate" });
            }

            return Ok(await _dashboardService.GetZplDailySummaryAsync(
                start, end, HttpContext?.RequestAborted ?? CancellationToken.None));
        }

        // GET: /api/zpl/latest
        // 返回最新状态列表（来自 ZPL_DataLatest），用于 zpl_list 页面
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] string? keyword = null)
        {
            var q = _db.ZPL_DataLatest.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(x =>
                    x.UserName.Contains(kw) ||
                    (x.Region ?? "").Contains(kw) ||
                    x.OrderNo.Contains(kw));
            }

            var data = await q
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.UserName,
                    x.Region,
                    x.OrderNo,
                    x.Timestamp
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: /api/zpl/history?userName=...&startDate=yyyy-MM-dd&endDate=yyyy-MM-dd&page=1&pageSize=10
        // 原生 SQL 服务端 GroupBy，避免 EF Core 复杂子查询翻译问题
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string userName,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest("userName 不能为空");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 10000) pageSize = 10000;

            var today = DateTime.Today;
            var start = ParseDateOrDefault(startDate, today.AddDays(-30));
            var endExclusive = ParseDateOrDefault(endDate, today).AddDays(1);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. 查询总记录数（按 OrderNo 分组后的数量）
            var countSql = @"
                SELECT COUNT(DISTINCT OrderNo)
                FROM ZPL_DataRecords
                WHERE UserName = @userName 
                  AND Timestamp >= @start 
                  AND Timestamp < @endExclusive";

            using var countCmd = new SqlCommand(countSql, conn);
            countCmd.Parameters.AddWithValue("@userName", userName);
            countCmd.Parameters.AddWithValue("@start", start);
            countCmd.Parameters.AddWithValue("@endExclusive", endExclusive);
            var totalCount = (int)(await countCmd.ExecuteScalarAsync() ?? 0);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 2. 查询当前页数据（GROUP BY + 子查询取最近非空 Region + OFFSET/FETCH 分页）
            var offset = (page - 1) * pageSize;
            var dataSql = @"
                SELECT 
                    OrderNo,
                    MIN(Timestamp) AS StartTime,
                    MAX(Timestamp) AS EndTime,
                    (SELECT TOP 1 sub.Region 
                     FROM ZPL_DataRecords sub 
                     WHERE sub.UserName = @userName 
                       AND sub.OrderNo = main.OrderNo
                       AND sub.Region IS NOT NULL
                       AND sub.Timestamp >= @start 
                       AND sub.Timestamp < @endExclusive
                     ORDER BY sub.Timestamp DESC) AS Region,
                    DATEDIFF(MINUTE, MIN(Timestamp), MAX(Timestamp)) AS DurationMinutes
                FROM ZPL_DataRecords main
                WHERE UserName = @userName 
                  AND Timestamp >= @start 
                  AND Timestamp < @endExclusive
                GROUP BY OrderNo
                ORDER BY MAX(Timestamp) DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            using var dataCmd = new SqlCommand(dataSql, conn);
            dataCmd.Parameters.AddWithValue("@userName", userName);
            dataCmd.Parameters.AddWithValue("@start", start);
            dataCmd.Parameters.AddWithValue("@endExclusive", endExclusive);
            dataCmd.Parameters.AddWithValue("@offset", offset);
            dataCmd.Parameters.AddWithValue("@pageSize", pageSize);

            var payload = new List<object>();
            using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                payload.Add(new
                {
                    OrderNo = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    Region = reader.IsDBNull(3) ? null : reader.GetString(3),
                    StartTime = reader.GetDateTime(1),
                    EndTime = reader.GetDateTime(2),
                    Duration = ToHumanDuration(reader.IsDBNull(4) ? 0 : reader.GetInt32(4))
                });
            }

            return Ok(new
            {
                data = payload,
                totalCount,
                totalPages,
                page,
                pageSize
            });

            static DateTime ParseDateOrDefault(string? s, DateTime def)
            {
                if (!string.IsNullOrWhiteSpace(s) &&
                    DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }
                return def;
            }

            static string ToHumanDuration(int minutes)
            {
                if (minutes < 0) minutes = 0;
                var ts = TimeSpan.FromMinutes(minutes);
                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours}小时{ts.Minutes}分";
                return $"{ts.Minutes}分";
            }
        }

        /// <summary>
        /// GET: /api/zpl/production-rate?hours=8
        /// 计算每个用户每小时产量（片/小时）
        /// 逻辑：找每个用户最新数据时间 → 回看指定小时窗口 → 按订单号去重取最新Quantity → 总数量/小时数
        /// </summary>
        [HttpGet("production-rate")]
        public async Task<IActionResult> GetProductionRate([FromQuery] int hours = 8)
        {
            if (hours <= 0 || hours > 24) hours = 8;

            var sql = @"
                WITH UserLatest AS (
                    -- 找到每个用户最新的数据时间点（自动往前找，不限于今天）
                    SELECT UserName, MAX(Timestamp) AS LatestTs
                    FROM ZPL_DataRecords
                    GROUP BY UserName
                ),
                DedupedOrders AS (
                    -- 从最新时间点往回看 N 小时，按订单号去重（同一单号取最新一条的 Quantity）
                    SELECT r.UserName, r.OrderNo,
                           MAX(r.Quantity) AS Quantity
                    FROM ZPL_DataRecords r
                    INNER JOIN UserLatest ul ON r.UserName = ul.UserName
                    WHERE r.Timestamp >= DATEADD(HOUR, -@hours, ul.LatestTs)
                      AND r.Timestamp <= ul.LatestTs
                    GROUP BY r.UserName, r.OrderNo
                )
                -- 汇总：总数量 / 小时数 = 片/小时
                SELECT UserName,
                       SUM(Quantity) AS TotalQuantity,
                       ROUND(CAST(SUM(Quantity) AS FLOAT) / @hours, 1) AS ProductionRatePerHour
                FROM DedupedOrders
                GROUP BY UserName";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@hours", hours);

            var result = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    UserName = reader.GetString(0),
                    TotalQuantity = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    ProductionRatePerHour = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2)
                });
            }

            return Ok(result);
        }

    }
}
