using System.ComponentModel.DataAnnotations;

namespace web_api2022.Models
{
    public class UpdateSettings
    {
        public string ReleaseRoot { get; set; }
        public string CurrentLatest { get; set; }
        public string UpdateLog { get; set; }
        public bool ForceUpdate { get; set; }
    }

    public class UpdateManifest
    {
        public string Version { get; set; }
        public List<FileItem> Files { get; set; }
    }

    public class FileItem
    {
        public string Path { get; set; }
        public string Hash { get; set; }
    }
    // 用于maximpost更新，用户账户模型，包含用户名、密码和 maximpost.json 的路径
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? Site { get; set; }

        // --- 以下是 MaximPost.json 的对应字段 ---

        public string Api { get; set; } = string.Empty;

        public string PrintShopId { get; set; } = string.Empty;

        public string BrandIdDefault { get; set; } = string.Empty;

        public string ProductionMode { get; set; } = "2";

        // 存储为逗号分隔的字符串，例如 "ADO001,ALS001..."
        public string BrandIds { get; set; } = string.Empty;

        // 存储为逗号分隔的字符串，例如 "Adolfo,AHLENS..."
        public string BrandNames { get; set; } = string.Empty;
        //账号备注
        public string? Note { get; set; }
    }
}
