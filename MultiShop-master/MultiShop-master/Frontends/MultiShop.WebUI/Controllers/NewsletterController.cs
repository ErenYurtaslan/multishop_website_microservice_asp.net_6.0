using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.NewsletterServices;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string returnUrl = null)
        {
            var outcome = await _newsletterService.SubscribeAsync();
            return await HandleOutcomeAsync(outcome, returnUrl, isSubscribe: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsubscribe(string returnUrl = null)
        {
            var outcome = await _newsletterService.UnsubscribeAsync();
            return await HandleOutcomeAsync(outcome, returnUrl, isSubscribe: false);
        }

        private async Task<IActionResult> HandleOutcomeAsync(NewsletterCallOutcome outcome, string returnUrl, bool isSubscribe)
        {
            switch (outcome)
            {
                case NewsletterCallOutcome.Ok:
                    TempData["NewsletterMessage"] = isSubscribe
                        ? "Bültenimize başarıyla abone oldun."
                        : "Bülten aboneliğinden çıktın.";
                    TempData["NewsletterMessageType"] = isSubscribe ? "success" : "info";
                    return RedirectSafe(returnUrl);

                case NewsletterCallOutcome.Reauthenticate:
                    // Eski oturum cookie'sinde yeni LocalApi scope'u yok. Logout + login
                    // sonrasi tekrar denemesi gerekiyor.
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["LoginNotice"] = "Sistem güncellendi, lütfen tekrar giriş yap. Ardından bülten aboneliğin aktifleşecek.";
                    var safeReturn = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                        ? returnUrl
                        : "/";
                    return Redirect($"/Login/Index?returnUrl={Uri.EscapeDataString(safeReturn)}");

                default:
                    TempData["NewsletterMessage"] = "Abonelik işlemi sırasında bir hata oluştu, lütfen tekrar dene.";
                    TempData["NewsletterMessageType"] = "warning";
                    return RedirectSafe(returnUrl);
            }
        }

        private IActionResult RedirectSafe(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Default");
        }
    }
}
