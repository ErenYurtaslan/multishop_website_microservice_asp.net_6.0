using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.CommentServices;
using Newtonsoft.Json;

namespace MultiShop.WebUI.ViewComponents.DefaultViewComponents
{
    public class _FeatureProductsDefaultComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly ICommentService _commentService;
        public _FeatureProductsDefaultComponentPartial(IProductService productService, IBrandService brandService, ICommentService commentService)
        {
            _productService = productService;
            _brandService = brandService;
            _commentService = commentService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _productService.GetAllProductAsync();
            values = (values ?? new List<ResultProductDto>())
                .Where(x => x != null)
                .OrderBy(_ => Guid.NewGuid())
                .Take(3)
                .ToList();

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
            return View(values);
        }

        public class ProductRatingSummary
        {
            public int ReviewCount { get; set; }
            public double AverageRating { get; set; }
        }
    }
}
