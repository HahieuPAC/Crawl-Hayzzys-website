using System.ComponentModel;

namespace CrawlDataWebsiteTool.Models
{
    public class ProductModel
    {
        [Description("Tên Sản Phẩm")] public string ProductName { get; set; }
        [Description("Phân loại")] public string ProductType { get; set; }
        [Description("Giá bán")] public string DiscountPrice { get; set; }
        [Description("Tiền Tệ")] public string Currency { get; set; }
        [Description("tên ảnh sản phẩm")] public string ProductNameImg { get; set; }
    }
}
