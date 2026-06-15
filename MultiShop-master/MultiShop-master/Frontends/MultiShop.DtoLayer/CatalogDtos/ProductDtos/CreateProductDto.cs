using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.CatalogDtos.ProductDtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductDescription { get; set; }
        public string Color { get; set; }
        [Newtonsoft.Json.JsonProperty("colorCode")]
        public string? ColorCode { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }
        public string CategoryId { get; set; }
        public string? BrandId { get; set; }
    }
}
