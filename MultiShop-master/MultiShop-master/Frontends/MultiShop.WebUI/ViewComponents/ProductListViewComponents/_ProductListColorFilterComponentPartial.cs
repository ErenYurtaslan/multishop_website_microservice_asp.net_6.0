using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.ViewComponents.ProductListViewComponents
{
    public class _ProductListColorFilterComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;

        public _ProductListColorFilterComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string selectedColor = null)
        {
            var colors = await _productService.GetDistinctColorsAsync();
            var products = await _productService.GetAllProductAsync();
            ViewBag.ColorHexByName = (products ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x?.Color))
                .GroupBy(x => x.Color.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ColorCode)
                          .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code)) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
            ViewBag.SelectedColor = selectedColor;
            return View(colors ?? new List<string>());
        }
    }
}
