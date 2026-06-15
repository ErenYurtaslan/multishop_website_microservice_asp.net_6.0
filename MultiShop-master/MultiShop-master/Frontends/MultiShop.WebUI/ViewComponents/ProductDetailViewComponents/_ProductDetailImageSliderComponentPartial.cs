using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Models.ViewModels;
using MultiShop.WebUI.Services.CatalogServices.ProductImageServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.ViewComponents.ProductDetailViewComponents
{
    public class _ProductDetailImageSliderComponentPartial : ViewComponent
    {
        private readonly IProductImageService _productImageService;
        private readonly IProductService _productService;

        public _ProductDetailImageSliderComponentPartial(
            IProductImageService productImageService,
            IProductService productService)
        {
            _productImageService = productImageService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var fromImages = await _productImageService.GetByProductIdProductImageAsync(id);
            var product = await _productService.GetByIdProductAsync(id);
            var fallback = NormalizeImageUrl(product?.ProductImageUrl);

            var vm = new ProductDetailSliderViewModel
            {
                Image1 = !string.IsNullOrWhiteSpace(fromImages?.Image1) ? NormalizeImageUrl(fromImages.Image1) : fallback,
                Image2 = NormalizeImageUrl(fromImages?.Image2, false),
                Image3 = NormalizeImageUrl(fromImages?.Image3, false),
                Image4 = NormalizeImageUrl(fromImages?.Image4, false)
            };
            if (string.IsNullOrWhiteSpace(vm.Image1))
                vm.Image1 = "/images/placeholder-product.svg";
            return View(vm);
        }

        private static string NormalizeImageUrl(string? rawUrl, bool usePlaceholderIfEmpty = true)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return usePlaceholderIfEmpty ? "/images/placeholder-product.svg" : string.Empty;
            }

            var value = rawUrl.Trim().Replace('\\', '/');
            if (value.StartsWith("~/", StringComparison.Ordinal))
            {
                value = value[1..];
            }

            if (value.StartsWith("//", StringComparison.Ordinal))
            {
                return $"https:{value}";
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) &&
                (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return absolute.ToString();
            }

            if (value.StartsWith("/", StringComparison.Ordinal))
            {
                return value;
            }

            return "/" + value.TrimStart('/');
        }
    }
}
