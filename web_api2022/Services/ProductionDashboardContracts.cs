namespace web_api2022.Services;

public interface IProductionDashboardService
{
    Task<DashboardSummary> GetDashboardAsync(DateOnly date, string? category, CancellationToken cancellationToken);

    Task<ZplDailySummary> GetZplDailySummaryAsync(DateOnly date, CancellationToken cancellationToken);
}

/// <summary>
/// 原始联点机台记录，供纯聚合辅助方法与测试使用。
/// </summary>
public sealed record LiandianRecord(DateTime Timestamp, int State);

/// <summary>
/// 单台联点机器的日汇总行。
/// </summary>
public sealed record MachineDailySummary(
    string MachineName,
    long ActiveSeconds,
    int ProductQuantity,
    int State,
    DateTime Timestamp);

/// <summary>
/// 联点分类（卷状 roll / 单片 single）的日汇总。
/// </summary>
public sealed record LiandianDailySummary(
    string Category,
    long ActiveSeconds,
    long TotalQuantity,
    int MachineCount,
    IReadOnlyList<MachineDailySummary> Machines);

/// <summary>
/// 单个 ZPL 用户的日结案产量汇总行。
/// </summary>
public sealed record ZplUserDailySummary(
    string UserName,
    int Quantity,
    string? OrderNo,
    string? Region,
    DateTime Timestamp);

/// <summary>
/// ZPL 日汇总。
/// </summary>
public sealed record ZplDailySummary(
    long TotalQuantity,
    int UserCount,
    IReadOnlyList<ZplUserDailySummary> Users);

/// <summary>
/// 生产看板总览：今日总产量 + 三个分类汇总。
/// </summary>
public sealed record DashboardSummary(
    long TotalQuantity,
    LiandianDailySummary Roll,
    LiandianDailySummary Single,
    ZplDailySummary Zpl);
