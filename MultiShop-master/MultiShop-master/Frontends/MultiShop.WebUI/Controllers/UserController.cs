using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.CargoServices.CargoCustomerServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var values = await _userService.GetUserInfo();
                if (values == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                return View(values);
            }
            catch
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}
