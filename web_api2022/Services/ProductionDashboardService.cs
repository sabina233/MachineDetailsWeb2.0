using Microsoft.EntityFrameworkCore;
using web_api2022.Data;

namespace web_api2022.Services;

public sealed class ProductionDashboardService : IProductionDashboardService
{
    private readonly DataContext _db;

    public ProductionDashboardService(DataContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 根据机台名归类：RFID-6 前缀为卷状机台，RFID-8 前缀为单片机台，其余返回 null。
    /// </summary>
    public static string? ClassifyMachine(string name) =>
        name.StartsWith("RFID-6", StringComparison.OrdinalIgnoreCase) ? "roll" :
        name.StartsWith("RFID-8", StringComparison.OrdinalIgnoreCase) ? "single" :
        null;

    /// <summary>
    /// 只累加 State == 1 的记录到下一条记录之间的时长，每个间隔上限 600 秒，
    /// 最后一条记录不向前推算时长，负间隔按 0 处理。
    /// </summary>
    public static long CalculateActiveSeconds(IReadOnlyList<LiandianRecord> records)
    {
        long total = 0;
        for (var i = 0; i + 1 < records.Count; i++)
        {
            if (records[i].State != 1)
            {
                continue;
            }

            var gapSeconds = (long)(records[i + 1].Timestamp - records[i].Timestamp).TotalSeconds;
            total += Math.Min(600, Math.Max(0, gapSeconds));
        }

        return total;
    }

    public async Task<DashboardSummary> GetDashboardAsync(
        DateOnly date, string? category, CancellationToken cancellationToken)
    {
        var roll = category is null or "roll"
            ? await BuildLiandianSummaryAsync(date, "roll", cancellationToken)
            : EmptyLiandianSummary("roll");

        var single = category is null or "single"
            ? await BuildLiandianSummaryAsync(date, "single", cancellationToken)
            : EmptyLiandianSummary("single");

        var zpl = category is null
            ? await GetZplDailySummaryAsync(date, cancellationToken)
            : new ZplDailySummary(0, 0, Array.Empty<ZplUserDailySummary>());

        return new DashboardSummary(
            roll.TotalQuantity + single.TotalQuantity + zpl.TotalQuantity,
            roll,
            single,
            zpl);
    }

    public async Task<ZplDailySummary> GetZplDailySummaryAsync(
        DateOnly date, CancellationToken cancellationToken)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var closedRecords = await _db.ZPL_DataRecords
            .Where(r => r.Timestamp >= start
                && r.Timestamp < end
                && r.PrintStatus == "Order closure successful")
            .Select(r => new { r.UserName, r.OrderNo, r.Quantity, r.Region, r.Timestamp })
            .ToListAsync(cancellationToken);

        // 每个 (UserName, OrderNo) 只取当天最新一笔 Quantity。
        var latestPerOrder = closedRecords
            .GroupBy(r => new { r.UserName, r.OrderNo })
            .Select(g => g.OrderByDescending(r => r.Timestamp).First())
            .ToList();

        var rows = latestPerOrder
            .GroupBy(r => r.UserName)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g =>
            {
                var latest = g.OrderByDescending(r => r.Timestamp).First();
                return new ZplUserDailySummary(
                    g.Key,
                    g.Sum(r => r.Quantity ?? 0),
                    latest.OrderNo,
                    latest.Region,
                    latest.Timestamp);
            })
            .ToList();

        return new ZplDailySummary(rows.Sum(r => (long)r.Quantity), rows.Count, rows);
    }

    private async Task<LiandianDailySummary> BuildLiandianSummaryAsync(
        DateOnly date, string category, CancellationToken cancellationToken)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var dayRecords = await _db.DataRecords
            .Where(r => r.Timestamp >= start && r.Timestamp < end)
            .Select(r => new LiandianRow(
                r.MachineName,
                r.Timestamp,
                r.Data.State,
                r.Data.TaskName,
                r.Data.ProductQuantity))
            .ToListAsync(cancellationToken);

        var machines = dayRecords
            .Where(r => ClassifyMachine(r.MachineName) == category)
            .GroupBy(r => r.MachineName)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g =>
            {
                var ordered = g.OrderBy(r => r.Timestamp).ToList();

                // 同一天内同一 (MachineName, TaskName) 只取最大 ProductQuantity，再按任务求和。
                var outputByTask = ordered
                    .Where(r => !string.IsNullOrWhiteSpace(r.TaskName))
                    .GroupBy(r => r.TaskName)
                    .Sum(tg => tg.Max(r => r.ProductQuantity));

                return new MachineDailySummary(
                    g.Key,
                    CalculateActiveSeconds(
                        ordered.Select(r => new LiandianRecord(r.Timestamp, r.State)).ToList()),
                    outputByTask,
                    ordered.Last().State,
                    ordered.Last().Timestamp);
            })
            .ToList();

        return new LiandianDailySummary(
            category,
            machines.Sum(m => m.ActiveSeconds),
            machines.Sum(m => (long)m.ProductQuantity),
            machines.Count,
            machines);
    }

    private static LiandianDailySummary EmptyLiandianSummary(string category)
        => new(category, 0, 0, 0, Array.Empty<MachineDailySummary>());
}

internal sealed record LiandianRow(
    string MachineName,
    DateTime Timestamp,
    int State,
    string TaskName,
    int ProductQuantity);
