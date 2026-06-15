namespace MultiShop.DtoLayer.CatalogDtos.ProductDtos
{
    public class ProductFilterRequestDto
    {
        public string? CategoryId { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }
        public string? Search { get; set; }
    }
}
