using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.CatalogDtos.ProductDtos
{
    public class ResultProductWithCategoryDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        [JsonProperty("productImageUrl")]
        public string ProductImageUrl { get; set; }
        public string ProductDescription { get; set; }
        public string Color { get; set; }
        [JsonProperty("colorCode")]
        public string? ColorCode { get; set; }
        public string Size { get; set; }
        [JsonProperty("stock")]
        public int Stock { get; set; }
        public string CategoryId { get; set; }
        [JsonProperty("brandId")]
        public string? BrandId { get; set; }
        public ResultCategoryDto Category { get; set; }
    }
}
