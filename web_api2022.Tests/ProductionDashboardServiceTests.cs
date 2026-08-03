using Xunit;
using Microsoft.EntityFrameworkCore;
using web_api2022.Data;
using web_api2022.Models;
using web_api2022.Services;

namespace web_api2022.Tests;

public class ProductionDashboardServiceTests
{
    [Theory]
    [InlineData("RFID-601", "roll")]
    [InlineData("rfid-601", "roll")]
    [InlineData("RFID-801", "single")]
    [InlineData("RFID-701", null)]
    public void ClassifyMachine_ReturnsExpectedCategory(string name, string? expected)
        => Assert.Equal(expected, ProductionDashboardService.ClassifyMachine(name));

    [Fact]
    public void CalculateActiveSeconds_counts_active_intervals_and_caps_each_gap_at_ten_minutes()
    {
        var records = new[]
        {
            new LiandianRecord(new DateTime(2026, 8, 3, 8, 0, 0), 1),
            new LiandianRecord(new DateTime(2026, 8, 3, 8, 3, 0), 0),
            new LiandianRecord(new DateTime(2026, 8, 3, 8, 5, 0), 1),
            new LiandianRecord(new DateTime(2026, 8, 3, 8, 25, 0), 1)
        };

        var activeSeconds = ProductionDashboardService.CalculateActiveSeconds(records);

        Assert.Equal(780, activeSeconds);
    }

    [Fact]
    public async Task GetLiandianSummary_UsesTaskMaximumAndExcludesOtherMachinePrefixes()
    {
        var db = CreateDb();
        var day = new DateTime(2026, 8, 3);
        db.DataRecords.AddRange(
            new DataRecordModel
            {
                MachineName = "RFID-601",
                Timestamp = day.AddHours(8),
                Message = "",
                CountryCode = "",
                Data = new NestedData { State = 1, TaskName = "A", FileName = "", ProductQuantity = 10 }
            },
            new DataRecordModel
            {
                MachineName = "RFID-601",
                Timestamp = day.AddHours(9),
                Message = "",
                CountryCode = "",
                Data = new NestedData { State = 1, TaskName = "A", FileName = "", ProductQuantity = 20 }
            },
            new DataRecordModel
            {
                MachineName = "RFID-601",
                Timestamp = day.AddHours(10),
                Message = "",
                CountryCode = "",
                Data = new NestedData { State = 1, TaskName = "B", FileName = "", ProductQuantity = 5 }
            },
            new DataRecordModel
            {
                MachineName = "RFID-801",
                Timestamp = day.AddHours(8),
                Message = "",
                CountryCode = "",
                Data = new NestedData { State = 1, TaskName = "A", FileName = "", ProductQuantity = 100 }
            },
            new DataRecordModel
            {
                MachineName = "RFID-701",
                Timestamp = day.AddHours(8),
                Message = "",
                CountryCode = "",
                Data = new NestedData { State = 1, TaskName = "A", FileName = "", ProductQuantity = 999 }
            });
        await db.SaveChangesAsync();

        var service = new ProductionDashboardService(db);
        var result = await service.GetDashboardAsync(new DateOnly(2026, 8, 3), "roll", default);

        Assert.Equal(25, result.Roll.TotalQuantity); // max(10,20) for A + 5 for B
        Assert.Equal(1, result.Roll.MachineCount);
        Assert.Single(result.Roll.Machines);
        Assert.Equal("RFID-601", result.Roll.Machines[0].MachineName);
        Assert.Equal(25, result.Roll.Machines[0].ProductQuantity);
    }

    [Fact]
    public async Task GetZplSummary_UsesLatestClosedRecordPerUserAndOrder()
    {
        var db = CreateDb();
        var day = new DateTime(2026, 8, 3);
        db.ZPL_DataRecords.AddRange(
            new ZPL_DataRecord
            {
                UserName = "U1",
                OrderNo = "O1",
                Quantity = 20,
                PrintStatus = "Order closure successful",
                Timestamp = day.AddHours(10)
            },
            new ZPL_DataRecord
            {
                UserName = "U1",
                OrderNo = "O1",
                Quantity = 30,
                PrintStatus = "Order closure successful",
                Timestamp = day.AddHours(11)
            },
            new ZPL_DataRecord
            {
                UserName = "U1",
                OrderNo = "O1",
                Quantity = 999,
                PrintStatus = "Pending",
                Timestamp = day.AddHours(12)
            },
            new ZPL_DataRecord
            {
                UserName = "U2",
                OrderNo = "O2",
                Quantity = 40,
                PrintStatus = "Order closure successful",
                Timestamp = day.AddHours(9)
            });
        await db.SaveChangesAsync();

        var service = new ProductionDashboardService(db);
        var result = await service.GetZplDailySummaryAsync(new DateOnly(2026, 8, 3), default);

        Assert.Equal(70, result.TotalQuantity); // 30 (latest U1/O1) + 40 (U2/O2)
        Assert.Equal(2, result.UserCount);
    }

    private static DataContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DataContext(options);
    }
}
