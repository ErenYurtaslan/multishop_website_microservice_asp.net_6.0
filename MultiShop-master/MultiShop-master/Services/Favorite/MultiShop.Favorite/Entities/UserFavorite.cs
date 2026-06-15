namespace MultiShop.Favorite.Entities
{
    public class UserFavorite
    {
        public int UserFavoriteId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
