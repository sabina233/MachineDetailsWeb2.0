using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace web_api2022.Models
{
    [Table("ZPL_DataLatest")]
    [Index(nameof(UserName), IsUnique = true)]
    public class ZPL_DataLatest
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 工号
        /// </summary>
        [MaxLength(50)]
        public string? EmployeeNo { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; } = null!;

        [MaxLength(100)]
        public string? Region { get; set; } // 可空

        [Required]
        public string OrderNo { get; set; } = null!;
        /// <summary>
        /// 款数
        /// </summary>
        public int? StyleCount { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 打印状态
        /// </summary>
        public string? PrintStatus { get; set; }

        /// <summary>
        /// 成品料号
        /// </summary>
        public string? MaterialNumber { get; set; }

        /// <summary>
        /// 当地时间
        /// </summary>
        public DateTimeOffset? LocalTime { get; set; }
        /// <summary>
        /// 电脑名称
        /// </summary>
        public string? ComputerName { get; set; }
        /// <summary>
        /// MAC地址
        /// </summary>
        public string? MacAddress { get; set; }
        /// <summary>
        /// 软件版本
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string? Country { get; set; }
        /// <summary>
        /// T100订单号
        /// </summary>
        public string? T100_Order { get; set; }
        /// <summary>
        /// ERP订单号
        /// </summary>
        public string? ERPNo { get; set; }
        /// <summary>
        /// Maxim订单号
        /// </summary>
        public string? MaximNo { get; set; }
        /// <summary>
        /// 尺寸
        /// </summary>
        public string? Size { get; set; }
        /// <summary>
        /// 版面设计
        /// </summary>
        public string? Layoutdesign { get; set; }
        /// <summary>
        /// 打印编号
        /// </summary>
        public string? PrintNo { get; set; }
        /// <summary>
        /// 加锁方式
        /// </summary>
        public string? Lock { get; set; }
        /// <summary>
        /// 站点ID
        /// </summary>
        public string? PrintshopID {  get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;  // 自动生成时间戳
    }

}
