using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.FavoriteDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.FavoriteServices;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IProductService _productService;

        public FavoriteController(IFavoriteService favoriteService, IProductService productService)
        {
            _favoriteService = favoriteService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Hesabım";
            ViewBag.directory3 = "Favorilerim";

            var values = await _favoriteService.GetMyFavoritesAsync();
            return View(values);
        }

        public async Task<IActionResult> Add(string id, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index", "Default");
            }

            var product = await _productService.GetByIdProductAsync(id);
            var dto = new CreateUserFavoriteDto
            {
                ProductId = id,
                ProductName = product?.ProductName ?? string.Empty,
                ProductImageUrl = product?.ProductImageUrl,
                ProductPrice = product?.ProductPrice ?? 0
            };

            await _favoriteService.AddFavoriteAsync(dto);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Favorite");
        }

        public async Task<IActionResult> Remove(int id, string? returnUrl = null)
        {
            await _favoriteService.RemoveFavoriteAsync(id);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Favorite");
        }

        public async Task<IActionResult> RemoveByProduct(string id, string? returnUrl = null)
        {
            await _favoriteService.RemoveFavoriteByProductAsync(id);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Favorite");
        }
    }
}
