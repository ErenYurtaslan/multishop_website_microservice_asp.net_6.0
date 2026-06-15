using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.CommentServices;
using Newtonsoft.Json;

namespace MultiShop.WebUI.ViewComponents.ProductDetailViewComponents
{
    public class _ProductDetailFeatureComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly ICommentService _commentService;
        public _ProductDetailFeatureComponentPartial(IProductService productService, IBrandService brandService, ICommentService commentService)
        {
            _productService = productService;
            _brandService = brandService;
            _commentService = commentService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var values=await _productService.GetByIdProductAsync(id);
            var comments = await _commentService.CommentListByProductId(id) ?? new();
            var reviewCount = comments.Count;
            var averageRating = reviewCount > 0 ? comments.Average(x => x.Rating) : 0d;
            ViewBag.ReviewCount = reviewCount;
            ViewBag.AverageRating = averageRating;

            if (!string.IsNullOrWhiteSpace(values?.BrandId))
            {
                var brands = await _brandService.GetAllBrandAsync();
                var brand = (brands ?? new()).FirstOrDefault(x => x.BrandId == values.BrandId);
                ViewBag.BrandName = brand?.BrandName;
                ViewBag.BrandLogo = brand?.ImageUrl;
            }
            return View(values);
        }
    }
}
