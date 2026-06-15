namespace MultiShop.DtoLayer.FavoriteDtos
{
    public class CreateUserFavoriteDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
