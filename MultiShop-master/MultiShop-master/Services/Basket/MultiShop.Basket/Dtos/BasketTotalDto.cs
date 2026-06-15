namespace MultiShop.Basket.Dtos
{
    public class BasketTotalDto
    {
        // UserId is server-assigned in controller from JWT claim.
        public string? UserId { get; set; }
        public string? DiscountCode { get; set; }
        public int DiscountRate { get; set; }
        public List<BasketItemDto> BasketItems { get; set; } = new();
        public decimal TotalPrice { get => BasketItems.Sum(x => x.Price * x.Quantity); }
    }
}