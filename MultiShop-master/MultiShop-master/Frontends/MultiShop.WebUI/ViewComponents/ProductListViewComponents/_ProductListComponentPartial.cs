using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.CommentServices;

namespace MultiShop.WebUI.ViewComponents.ProductListViewComponents
{
    public class _ProductListComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly ICommentService _commentService;
        public _ProductListComponentPartial(IProductService productService, IBrandService brandService, ICommentService commentService)
        {
            _productService = productService;
            _brandService = brandService;
            _commentService = commentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, ProductFilterRequestDto filter = null)
        {
            filter ??= new ProductFilterRequestDto();

            // If category id was provided positionally and filter doesn't already carry it, copy it.
            if (!string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(filter.CategoryId))
            {
                filter.CategoryId = id;
            }

            var hasAnyFilter =
                !string.IsNullOrWhiteSpace(filter.CategoryId) ||
                !string.IsNullOrWhiteSpace(filter.Color) ||
                !string.IsNullOrWhiteSpace(filter.Size) ||
                filter.MinPrice.HasValue ||
                filter.MaxPrice.HasValue ||
                filter.InStockOnly ||
                !string.IsNullOrWhiteSpace(filter.Search);

            List<ResultProductWithCategoryDto> values;
            if (hasAnyFilter)
            {
                values = await _productService.GetFilteredProductsAsync(filter);
            }
            else
            {
                values = await _productService.GetProductsWithCategoryAsync();
            }

            var brands = await _brandService.GetAllBrandAsync();
            ViewBag.BrandLookup = (brands ?? new()).ToDictionary(x => x.BrandId, x => x);
            var comments = await _commentService.GetAllCommentAsync();
            ViewBag.RatingSummaryByProductId = (comments ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductId))
                .GroupBy(x => x.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => new ProductRatingSummary
                    {
                        ReviewCount = g.Count(),
                        AverageRating = g.Average(x => x.Rating)
                    });

            return View(values ?? new List<ResultProductWithCategoryDto>());
        }

        public class ProductRatingSummary
        {
            public int ReviewCount { get; set; }
            public double AverageRating { get; set; }
        }
    }
}
