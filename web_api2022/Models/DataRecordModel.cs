using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace web_api2022.Models
{

        [Index(nameof(MachineName), nameof(Timestamp))]
        [Index(nameof(Timestamp))]
        public class DataRecordModel
        {
            [Key]
            public int Id { get; set; }  // 主键，自动生成

            [Required]
            public DateTime Timestamp { get; set; } = DateTime.Now;  // 自动生成时间戳

            [Required, MaxLength(200)]
            public string MachineName { get; set; }

            public int Code { get; set; }

            [MaxLength(500)]
            public string Message { get; set; }
            //国家代码
            [MaxLength(50)]
            public string CountryCode{get;set;}

            // 嵌套的 DataDetails，用来接收 "data" 中的字段
            public NestedData Data { get; set; }
        }

        public class NestedData
        {
            public int State { get; set; }

            public int Speed { get; set; }

            public int SetSpeed { get; set; }

            [MaxLength(500)]
            public string TaskName { get; set; }

            public int TaskNum { get; set; }

            public int ResidueTask { get; set; }

            public int CardReplace { get; set; }

            [MaxLength(500)]
            public string FileName { get; set; }

            public int TotalRecords { get; set; }

            public int ProductQuantity { get; set; }

            public int TotalSuccess { get; set; }

            public int TotalFail { get; set; }

            public int BatchNum { get; set; }

            public int BatchSuccess { get; set; }

            public int BatchFail { get; set; }

            public int Trigger1 { get; set; }

            public int Trigger2 { get; set; }

            public int Inventory1 { get; set; }

            public int Inventory2 { get; set; }

            public int Discard1 { get; set; }

            public int Discard2 { get; set; }

            public float RejectRate { get; set; }

            public int DataErr { get; set; }
        }
    
}
