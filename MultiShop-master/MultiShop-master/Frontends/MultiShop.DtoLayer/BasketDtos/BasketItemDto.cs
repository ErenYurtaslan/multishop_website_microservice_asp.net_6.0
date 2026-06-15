using Newtonsoft.Json;

namespace MultiShop.DtoLayer.BasketDtos
{
    public class BasketItemDto
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonProperty("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("productImageUrl")]
        public string? ProductImageUrl { get; set; }
    }
}
