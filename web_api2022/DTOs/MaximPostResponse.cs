namespace web_api2022.DTOs
{
    public class MaximPostResponse
    {
        public string Api { get; set; } = string.Empty;
        public string PrintShopId { get; set; } = string.Empty;
        public string BrandId_default { get; set; } = string.Empty;
        public string ProductionMode { get; set; } = string.Empty;
        public List<string> BrandId { get; set; } = new();
        public List<string> BrandName { get; set; } = new();
    }
}
