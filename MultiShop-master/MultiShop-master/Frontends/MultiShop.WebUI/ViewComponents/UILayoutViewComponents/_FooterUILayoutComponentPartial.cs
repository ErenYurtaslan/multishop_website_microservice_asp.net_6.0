using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.AboutDtos;
using MultiShop.WebUI.Services.CatalogServices.AboutServices;
using MultiShop.WebUI.Services.NewsletterServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace MultiShop.WebUI.ViewComponents.UILayoutViewComponents
{
    public class _FooterUILayoutComponentPartial : ViewComponent
    {
        private readonly IAboutService _aboutService;
        private readonly INewsletterService _newsletterService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public _FooterUILayoutComponentPartial(
            IAboutService aboutService,
            INewsletterService newsletterService,
            IHttpContextAccessor httpContextAccessor)
        {
            _aboutService = aboutService;
            _newsletterService = newsletterService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var values = await _aboutService.GetAllAboutAsync();

            var user = _httpContextAccessor.HttpContext?.User;
            ViewBag.NewsletterStatus = null;
            ViewBag.NewsletterReauthRequired = false;

            if (user?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var result = await _newsletterService.GetStatusAsync();
                    if (result?.Outcome == NewsletterCallOutcome.Ok)
                    {
                        ViewBag.NewsletterStatus = result.Status;
                    }
                    else if (result?.Outcome == NewsletterCallOutcome.Reauthenticate)
                    {
                        ViewBag.NewsletterReauthRequired = true;
                    }
                }
                catch
                {
                    // sessizce yutmak: footer hiç render edilmemesinden iyidir.
                }
            }

            return View(values);
        }
    }
}
