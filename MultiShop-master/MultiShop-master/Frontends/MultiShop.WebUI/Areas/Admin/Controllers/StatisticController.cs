using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CommentServices;
using MultiShop.WebUI.Services.OrderServices.OrderOderingServices;
using MultiShop.WebUI.Services.StatisticServices.CatalogStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.DiscountStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.MessageStatisticServices;
using MultiShop.WebUI.Services.StatisticServices.UserStatisticServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class StatisticController : Controller
    {
        private readonly ICatalogStatisticService _catalogStatisticService;
        private readonly IUserStatisticService _userStatisticService;
        private readonly ICommentService _commentService;
        private readonly IDiscountStatisticService _discountStatisticService;
        private readonly IMessageStatisticService _messageStatisticService;
        private readonly IOrderOderingService _orderOderingService;
        private readonly IUserService _userService;
        private readonly IBrandService _brandService;
        public StatisticController(
            ICatalogStatisticService catalogStatisticService,
            IUserStatisticService userStatisticService,
            ICommentService commentService,
            IDiscountStatisticService discountStatisticService,
            IMessageStatisticService messageStatisticService,
            IOrderOderingService orderOderingService,
            IUserService userService,
            IBrandService brandService)
        {
            _catalogStatisticService = catalogStatisticService;
            _userStatisticService = userStatisticService;
            _commentService = commentService;
            _discountStatisticService = discountStatisticService;
            _messageStatisticService = messageStatisticService;
            _orderOderingService = orderOderingService;
            _userService = userService;
            _brandService = brandService;
        }

        public async Task<IActionResult> Index()
        {
            var getBrandCount = await _catalogStatisticService.GetBrandCount();
            var getProductCount = await _catalogStatisticService.GetProductCount();
            var getCategoryCount = await _catalogStatisticService.GetCategoryCount();
            var getMaxPriceProductName = await _catalogStatisticService.GetMaxPriceProductName();
            var getMinPriceProductName = await _catalogStatisticService.GetMinPriceProductName();
            var getProductAvgPrice = await _catalogStatisticService.GetProductAvgPrice();

            var getUserCount = await _userStatisticService.GetUsercount();

            var getTotalCommentCount = await _commentService.GetTotalCommentCount();
            var getActiveCommentCount = await _commentService.GetActiveCommentCount();
            var getPassiveCommentCount = await _commentService.GetPAssiveCommentCount();

            var getDiscountCouponCount = await _discountStatisticService.GetDiscountCouponCount();

            var getMessageTotalCount = await _messageStatisticService.GetTotalMessageCount();

            var allOrderings = await _orderOderingService.GetAllOrderingsAsync();
            var totalOrderCount = allOrderings.Count;
            var orderingCustomerCount = allOrderings
                .Where(x => !string.IsNullOrWhiteSpace(x.UserId))
                .Select(x => x.UserId)
                .Distinct()
                .Count();
            var totalRevenue = allOrderings.Sum(x => x.TotalPrice);
            var today = DateTime.Today;
            var todayRevenue = allOrderings
                .Where(x => x.OrderDate.Date == today)
                .Sum(x => x.TotalPrice);
            var lastSaleAmount = allOrderings
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.OrderingId)
                .Select(x => x.TotalPrice)
                .FirstOrDefault();

            var topCustomerName = "-";
            if (allOrderings.Count > 0)
            {
                var topUserId = allOrderings
                    .GroupBy(x => x.UserId)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .Select(g => g.Key)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(topUserId))
                {
                    var users = await _userService.GetAllUserList();
                    var topUser = users.FirstOrDefault(x => x.Id == topUserId);
                    if (topUser != null)
                    {
                        topCustomerName = $"{topUser.Name} {topUser.Surname}".Trim();
                        if (string.IsNullOrWhiteSpace(topCustomerName))
                        {
                            topCustomerName = topUser.Email ?? topUser.Username ?? topUserId;
                        }
                    }
                    else
                    {
                        topCustomerName = topUserId;
                    }
                }
            }

            var brands = await _brandService.GetAllBrandAsync();
            var lastBrandName = brands
                .OrderBy(x => x.BrandId)
                .Select(x => x.BrandName)
                .LastOrDefault() ?? "-";

            ViewBag.getBrandCount = getBrandCount;
            ViewBag.getProductCount = getProductCount;
            ViewBag.getCategoryCount = getCategoryCount;
            ViewBag.getMaxPriceProductName = getMaxPriceProductName;
            ViewBag.getMinPriceProductName = getMinPriceProductName;
            ViewBag.getProductAvgPrice = getProductAvgPrice;

            ViewBag.getUserCount = getUserCount;

            ViewBag.getTotalCommentCount = getTotalCommentCount;
            ViewBag.getActiveCommentCount = getActiveCommentCount;
            ViewBag.getPassiveCommentCount = getPassiveCommentCount;

            ViewBag.getDiscountCouponCount = getDiscountCouponCount;

            ViewBag.getMessageTotalCount = getMessageTotalCount;
            ViewBag.getTopCustomerName = topCustomerName;
            ViewBag.getTotalOrderCount = totalOrderCount;
            ViewBag.getOrderingCustomerCount = orderingCustomerCount;
            ViewBag.getLastBrandName = lastBrandName;
            ViewBag.getTotalRevenue = totalRevenue;
            ViewBag.getTodayRevenue = todayRevenue;
            ViewBag.getLastSaleAmount = lastSaleAmount;

            return View();
        }
    }
}
