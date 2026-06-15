namespace MultiShop.WebUI.Areas.User.Models
{
    public class UserOrderProductPreviewViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = "/images/placeholder-product.svg";
        public int Quantity { get; set; }
    }
}
