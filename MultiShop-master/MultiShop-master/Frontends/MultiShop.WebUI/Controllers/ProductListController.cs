using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.DtoLayer.CommentDtos;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Controllers
{
    public class ProductListController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public ProductListController(ICommentService commentService, IUserService userService)
        {
            _commentService = commentService;
            _userService = userService;
        }

        public IActionResult Index(
            string id,
            string color = null,
            string size = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool inStockOnly = false,
            string search = null)
        {
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Ürünler";
            ViewBag.directory3 = "Ürün Listesi";
            ViewBag.i = id;

            var filter = new ProductFilterRequestDto
            {
                CategoryId = id,
                Color = color,
                Size = size,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStockOnly,
                Search = search
            };
            ViewBag.Filter = filter;

            return View();
        }

        public async Task<IActionResult> ProductDetail(string id)
        {
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Ürün Listesi";
            ViewBag.directory3 = "Ürün Detayları";
            ViewBag.x = id;
            ViewBag.CommentRating = TempData["CommentRating"]?.ToString() ?? "5";
            ViewBag.CommentDetail = TempData["CommentDetail"]?.ToString() ?? string.Empty;

            if (User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var user = await _userService.GetUserInfo();
                    if (user != null)
                    {
                        var defaultName = $"{user.Name} {user.Surname}".Trim();
                        ViewBag.CommentNameSurname = TempData["CommentNameSurname"]?.ToString() ?? defaultName;
                        ViewBag.CommentEmail = TempData["CommentEmail"]?.ToString() ?? (user.Email ?? string.Empty);
                    }
                }
                catch
                {
                    // Kullanıcı bilgisi alınamazsa form boş gelir.
                }
            }
            else
            {
                ViewBag.CommentNameSurname = TempData["CommentNameSurname"]?.ToString() ?? string.Empty;
                ViewBag.CommentEmail = TempData["CommentEmail"]?.ToString() ?? string.Empty;
            }

            return View();
        }

        [HttpGet]
        public PartialViewResult AddComment(string id)
        {
            ViewBag.ProductId = id;
            return PartialView();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateCommentDto createCommentDto)
        {
            if (string.IsNullOrWhiteSpace(createCommentDto.ProductId))
            {
                ModelState.AddModelError(string.Empty, "Ürün bilgisi eksik, yorum eklenemedi.");
                return RedirectToAction("Index", "Default");
            }

            if (createCommentDto.Rating < 1) createCommentDto.Rating = 5;
            if (createCommentDto.Rating > 5) createCommentDto.Rating = 5;

            if (string.IsNullOrWhiteSpace(createCommentDto.NameSurname) ||
                string.IsNullOrWhiteSpace(createCommentDto.Email) ||
                string.IsNullOrWhiteSpace(createCommentDto.CommentDetail))
            {
                TempData["CommentRating"] = createCommentDto.Rating.ToString();
                TempData["CommentNameSurname"] = createCommentDto.NameSurname ?? string.Empty;
                TempData["CommentEmail"] = createCommentDto.Email ?? string.Empty;
                TempData["CommentDetail"] = createCommentDto.CommentDetail ?? string.Empty;
                return RedirectToAction("ProductDetail", "ProductList", new { id = createCommentDto.ProductId });
            }

            createCommentDto.ImageUrl = string.IsNullOrWhiteSpace(createCommentDto.ImageUrl) ? "default" : createCommentDto.ImageUrl;
            createCommentDto.CreatedDate = DateTime.Now;
            createCommentDto.Status = false;

            try
            {
                await _commentService.CreateCommentAsync(createCommentDto);
            }
            catch
            {
                TempData["CommentRating"] = createCommentDto.Rating.ToString();
                TempData["CommentNameSurname"] = createCommentDto.NameSurname ?? string.Empty;
                TempData["CommentEmail"] = createCommentDto.Email ?? string.Empty;
                TempData["CommentDetail"] = createCommentDto.CommentDetail ?? string.Empty;
            }

            return RedirectToAction("ProductDetail", "ProductList", new { id = createCommentDto.ProductId });
        }
    }
}
