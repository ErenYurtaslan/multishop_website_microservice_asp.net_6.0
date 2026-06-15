namespace MultiShop.DtoLayer.FavoriteDtos
{
    public class ResultUserFavoriteDto
    {
        public int UserFavoriteId { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
