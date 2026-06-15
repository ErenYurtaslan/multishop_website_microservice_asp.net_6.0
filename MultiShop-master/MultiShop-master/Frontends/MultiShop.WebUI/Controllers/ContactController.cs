using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ContactDtos;
using MultiShop.WebUI.Services.CatalogServices.ContactServices;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IUserService _userService;
        public ContactController(IContactService contactService, IUserService userService)
        {
            _contactService = contactService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "İletişim";
            ViewBag.directory3 = "Mesaj Gönder";

            var model = new CreateContactDto();
            if (User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var user = await _userService.GetUserInfo();
                    if (user != null)
                    {
                        model.NameSurname = $"{user.Name} {user.Surname}".Trim();
                        model.Email = user.Email ?? string.Empty;
                    }
                }
                catch
                {
                    // Loginli olsa da profil cekimi basarisiz olursa form bos gelir.
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateContactDto createContactDto)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var user = await _userService.GetUserInfo();
                    if (user != null)
                    {
                        if (string.IsNullOrWhiteSpace(createContactDto.NameSurname))
                            createContactDto.NameSurname = $"{user.Name} {user.Surname}".Trim();
                        if (string.IsNullOrWhiteSpace(createContactDto.Email))
                            createContactDto.Email = user.Email ?? string.Empty;
                    }
                }
                catch
                {
                    // Form verisiyle devam ederiz.
                }
            }

            if (string.IsNullOrWhiteSpace(createContactDto.NameSurname) ||
                string.IsNullOrWhiteSpace(createContactDto.Email) ||
                string.IsNullOrWhiteSpace(createContactDto.Subject) ||
                string.IsNullOrWhiteSpace(createContactDto.Message))
            {
                ModelState.AddModelError(string.Empty, "Lütfen tüm zorunlu alanları doldur.");
                ViewBag.directory1 = "MultiShop";
                ViewBag.directory2 = "İletişim";
                ViewBag.directory3 = "Mesaj Gönder";
                return View(createContactDto);
            }

            createContactDto.IsRead = false;
            createContactDto.SendDate = DateTime.Now;
            await _contactService.CreateContactAsync(createContactDto);
            TempData["ContactSuccess"] = "Mesajınız başarıyla gönderildi.";
            return RedirectToAction("Index", "Contact");
        }
    }
}
