namespace MultiShop.Basket.Dtos
{
    public class BasketItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
