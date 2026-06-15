using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;
using MultiShop.WebUI.Services.FavoriteServices;

namespace MultiShop.WebUI.ViewComponents.UILayoutViewComponents
{
    public class _NavbarUILayoutComponentPartial : ViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly IFavoriteService _favoriteService;
        private readonly IBasketService _basketService;

        public _NavbarUILayoutComponentPartial(ICategoryService categoryService, IFavoriteService favoriteService, IBasketService basketService)
        {
            _categoryService = categoryService;
            _favoriteService = favoriteService;
            _basketService = basketService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllCategoryAsync();

            var favoriteCount = 0;
            var basketCount = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    favoriteCount = await _favoriteService.GetMyFavoritesCountAsync();
                }
                catch
                {
                    favoriteCount = 0;
                }

                try
                {
                    var basket = await _basketService.GetBasket();
                    basketCount = basket?.BasketItems?.Sum(x => x.Quantity) ?? 0;
                }
                catch
                {
                    basketCount = 0;
                }
            }
            ViewBag.FavoriteCount = favoriteCount;
            ViewBag.BasketCount = basketCount;

            return View(categories);
        }
    }
}
