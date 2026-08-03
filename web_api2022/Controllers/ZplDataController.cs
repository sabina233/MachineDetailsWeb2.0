using Microsoft.AspNetCore.Mvc;
using System;
using web_api2022.Models;
using Microsoft.EntityFrameworkCore;
using web_api2022.Data;
using web_api2022.Models;
using web_api2022.Configurations;
using Microsoft.Extensions.Options;


namespace web_api2022.Controllers
{
    [ApiController]
    [Route("api/zpl-data")]
    public class ZplDataController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly string _apiKey;


        public ZplDataController(DataContext db, IOptions<ApiSettings> settings)
        {
            _db = db;
            _apiKey = settings.Value.ApiKey;
        }


        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ZPL_DataRecord input)
        {
            // 验证 API Key
            if (!Request.Headers.TryGetValue("ZPL_KEY", out var extractedApiKey))
                return Unauthorized(new { message = "API Key is missing" });

            if (!_apiKey.Equals(extractedApiKey))
                return Unauthorized(new { message = "Invalid API Key" });


            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 设置时间戳为当前时间
            input.Timestamp = DateTime.Now;

            //添加日志写入当前时间戳和当地localtime时间戳
            // 直接在控制台输出日志信息
            Console.WriteLine("New record received:");
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Timestamp: {input.Timestamp}");
            Console.WriteLine($"LocalTime: {input.LocalTime}");
            Console.WriteLine($"ComputerName: {input.ComputerName}");
            Console.WriteLine($"MacAddress: {input.MacAddress}");
            Console.WriteLine($"IpAddress: {input.IpAddress}");
            Console.WriteLine("----------------------------------");

            // 写入 ZPL_DataRecord（所有记录都保存）
            _db.ZPL_DataRecords.Add(input);

            // 写入 ZPL_DataLatest（只保留每个用户名最新记录）
            var existing = await _db.ZPL_DataLatest
                .FirstOrDefaultAsync(x => x.UserName == input.UserName);

            if (existing == null)
            {
                // 没有记录，插入新记录
                var latest = new ZPL_DataLatest
                {
                    UserName = input.UserName,
                    Region = input.Region,
                    OrderNo = input.OrderNo,
                    Timestamp = input.Timestamp,
                    EmployeeNo = input.EmployeeNo,
                    StyleCount = input.StyleCount,
                    Quantity = input.Quantity,
                    MaterialNumber = input.MaterialNumber,
                    LocalTime = input.LocalTime,
                    ComputerName = input.ComputerName,
                    MacAddress = input.MacAddress,
                    Version = input.Version,
                    IpAddress = input.IpAddress,
                    Country = input.Country,
                    T100_Order = input.T100_Order,
                    ERPNo = input.ERPNo,
                    MaximNo = input.MaximNo,
                    Size = input.Size,
                    Layoutdesign = input.Layoutdesign,
                    PrintNo = input.PrintNo,
                    Lock = input.Lock,
                    PrintshopID = input.PrintshopID
                };
                _db.ZPL_DataLatest.Add(latest);
            }
            else if (input.Timestamp > existing.Timestamp)
            {
                // 有记录但时间更新，覆盖旧记录
                existing.Region = input.Region;
                existing.OrderNo = input.OrderNo;
                existing.Timestamp = input.Timestamp;
                existing.EmployeeNo = input.EmployeeNo;
                existing.StyleCount = input.StyleCount;
                existing.Quantity = input.Quantity;
                existing.MaterialNumber = input.MaterialNumber;
                existing.LocalTime = input.LocalTime;
                existing.ComputerName = input.ComputerName;
                existing.MacAddress = input.MacAddress;
                existing.Version = input.Version;
                existing.IpAddress = input.IpAddress;
                existing.Country = input.Country;
                existing.T100_Order = input.T100_Order;
                existing.ERPNo = input.ERPNo;
                existing.MaximNo = input.MaximNo;
                existing.Size = input.Size;
                existing.Layoutdesign = input.Layoutdesign;
                existing.PrintNo = input.PrintNo;
                existing.Lock = input.Lock;
                existing.PrintshopID = input.PrintshopID;
                _db.ZPL_DataLatest.Update(existing);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "记录已写入", timestamp = input.Timestamp });
        }
    }

}
