using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MultiShop.DtoLayer.BasketDtos
{
    public class BasketTotalDto
    {
        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("discountCode")]
        public string? DiscountCode { get; set; }

        [JsonProperty("discountRate")]
        public int DiscountRate { get; set; }

        [JsonProperty("basketItems")]
        public List<BasketItemDto> BasketItems { get; set; } = new();

        [JsonIgnore]
        public decimal TotalPrice { get => BasketItems?.Sum(x => x.Price * x.Quantity) ?? 0; }
    }
}
