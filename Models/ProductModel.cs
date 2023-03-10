using System.ComponentModel;

namespace CrawlDataWebsiteTool.Models
{
    public class ProductModel
    {
        [Description("STT")] public string ProducOrder { get; set; }
        [Description("Tên Sản Phẩm")] public string ProductName { get; set; }
        [Description("Phân loại")] public string ProductType { get; set; }
        [Description("Giới thiệu sản phẩm")] public string IntroProduct { get; set; }
        [Description("Mã sản phẩm")] public string ProductCode { get; set; }
        [Description("Chất liệu")] public string Meterial { get; set; }
        [Description("Trọng lượng")] public string Mass { get; set; }
        [Description("Giá bán")] public string DiscountPrice { get; set; }
        [Description("Giá gốc")] public string OriginPrice { get; set; }
        [Description("Chênh lệch")] public string Retail { get; set; }
        [Description("Tiền Tệ")] public string Currency { get; set; }
    }
}
