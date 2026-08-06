using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using web_api2022.Configurations;
using web_api2022.Controllers;
using web_api2022.Services;
using Xunit;

namespace web_api2022.Tests;

public class DashboardEndpointTests
{
    [Fact]
    public async Task Summary_WithInvalidDate_ReturnsBadRequest()
    {
        var controller = CreateDataController();

        var result = await controller.GetDashboardSummary("2026-13-99", "2026-08-04");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Liandian_WithUnknownCategory_ReturnsBadRequest()
    {
        var controller = CreateDataController();

        var result = await controller.GetLiandianDashboard("2026-08-03", "2026-08-03", "bogus");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ZplSummary_ReturnsTotalAndUsers()
    {
        var controller = CreateZplController();

        var result = await controller.GetDailySummary("2026-08-03", "2026-08-03");

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<ZplDailySummary>(ok.Value);
        Assert.Equal(70, summary.TotalQuantity);
        Assert.Equal(2, summary.UserCount);
    }

    private static DataController CreateDataController()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        return new DataController(
            null!,
            null!,
            Options.Create(new ApiSettings()),
            config,
            new StubDashboardService());
    }

    private static ZplQueryController CreateZplController()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        return new ZplQueryController(null!, config, new StubDashboardService());
    }

    private sealed class StubDashboardService : IProductionDashboardService
    {
        public Task<DashboardSummary> GetDashboardAsync(
            DateOnly startDate, DateOnly endDate, string? category, CancellationToken cancellationToken)
            => Task.FromResult(new DashboardSummary(
                0,
                new LiandianDailySummary("roll", 0, 0, 0, Array.Empty<MachineDailySummary>()),
                new LiandianDailySummary("single", 0, 0, 0, Array.Empty<MachineDailySummary>()),
                new ZplDailySummary(0, 0, Array.Empty<ZplUserDailySummary>())));

        public Task<ZplDailySummary> GetZplDailySummaryAsync(
            DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
            => Task.FromResult(new ZplDailySummary(
                70,
                2,
                new[]
                {
                    new ZplUserDailySummary("U1", 30, "O1", null, new DateTime(2026, 8, 3, 11, 0, 0)),
                    new ZplUserDailySummary("U2", 40, "O2", null, new DateTime(2026, 8, 3, 9, 0, 0))
                }));
    }
}
