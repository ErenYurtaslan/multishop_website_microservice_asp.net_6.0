using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.OrderServices.OrderAddressServices;
using MultiShop.WebUI.Services.UserIdentityServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/User")]
    public class UserController : Controller
    {
        private readonly IUserIdentityService _userIdentityService;
        private readonly IOrderAddressService _orderAddressService;
        public UserController(IUserIdentityService userIdentityService, IOrderAddressService orderAddressService)
        {
            _userIdentityService = userIdentityService;
            _orderAddressService = orderAddressService;
        }

        [HttpGet("UserList")]
        public async Task<IActionResult> UserList()
        {
            var values = await _userIdentityService.GetAllUserListAsync();
            return View(values);
        }

        [HttpPost("ResetUserPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["AdminUserError"] = "Şifre sıfırlamak için geçerli kullanıcı seçilmelidir.";
                return RedirectToAction(nameof(UserList));
            }

            const string defaultPassword = "Aa123456!";
            var ok = await _userIdentityService.AdminResetPasswordAsync(id, defaultPassword);
            TempData["AdminUserError"] = ok
                ? $"Şifre başarıyla sıfırlandı. Yeni şifre: {defaultPassword}"
                : "Şifre sıfırlama sırasında hata oluştu.";
            return RedirectToAction(nameof(UserList));
        }

        [HttpGet("UserAddressInfo/{id?}")]
        public async Task<IActionResult> UserAddressInfo(string? id, string? email)
        {
            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(email))
            {
                TempData["AdminUserError"] = "Adres detayı için kullanıcı seçmelisiniz.";
                return RedirectToAction(nameof(UserList));
            }

            var resolvedEmail = email;
            if (string.IsNullOrWhiteSpace(resolvedEmail) && !string.IsNullOrWhiteSpace(id))
            {
                var users = await _userIdentityService.GetAllUserListAsync();
                resolvedEmail = users?.FirstOrDefault(x => x.id == id)?.email;
            }

            var values = await _orderAddressService.GetDistinctAddressesByEmailAsync(resolvedEmail ?? string.Empty);
            if (values == null || values.Count == 0)
            {
                TempData["AdminUserError"] = "Bu kullanıcı için sipariş adres kaydı bulunamadı.";
                return RedirectToAction(nameof(UserList));
            }

            ViewBag.AddressInfoEmail = resolvedEmail;
            return View(values);
        }
    }
}
