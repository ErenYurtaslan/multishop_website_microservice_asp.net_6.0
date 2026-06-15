using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.BasketServices;

namespace MultiShop.WebUI.ViewComponents.ShoppingCartViewComponents
{
    public class _ShoppingCartProductListComponentPartial : ViewComponent
    {
        private readonly IBasketService _basketService;
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        public _ShoppingCartProductListComponentPartial(
            IBasketService basketService,
            IProductService productService,
            IBrandService brandService)
        {
            _basketService = basketService;
            _productService = productService;
            _brandService = brandService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basketTotal = await _basketService.GetBasket();
            var basketItems = basketTotal.BasketItems ?? new();

            var products = await _productService.GetAllProductAsync();
            var brands = await _brandService.GetAllBrandAsync();
            var productBrandMap = (products ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x?.ProductId))
                .ToDictionary(x => x.ProductId!, x => x.BrandId);
            var brandLookup = (brands ?? new()).ToDictionary(x => x.BrandId, x => x);

            ViewBag.BrandByProductId = basketItems
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductId))
                .ToDictionary(
                    x => x.ProductId,
                    x =>
                    {
                        if (productBrandMap.TryGetValue(x.ProductId, out var brandId) &&
                            !string.IsNullOrWhiteSpace(brandId) &&
                            brandLookup.TryGetValue(brandId, out var brand))
                        {
                            return brand;
                        }
                        return null;
                    });

            return View(basketItems);
        }
    }
}
