using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.ViewComponents.ProductListViewComponents
{
    public class _ProductListSizeFilterComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;

        public _ProductListSizeFilterComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string selectedSize = null)
        {
            var sizes = await _productService.GetDistinctSizesAsync();
            ViewBag.SelectedSize = selectedSize;
            return View(sizes ?? new List<string>());
        }
    }
}
