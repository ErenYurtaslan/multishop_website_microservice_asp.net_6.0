using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using MultiShop.DtoLayer.IdentityDtos.RegisterDtos;
using MultiShop.WebUI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MultiShop.WebUI.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIdentityService _identityService;
        public RegisterController(IHttpClientFactory httpClientFactory, IIdentityService identityService)
        {
            _httpClientFactory = httpClientFactory;
            _identityService = identityService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateRegisterDto createRegisterDto)
        {
            if (createRegisterDto == null
                || string.IsNullOrWhiteSpace(createRegisterDto.Username)
                || string.IsNullOrWhiteSpace(createRegisterDto.Email)
                || string.IsNullOrWhiteSpace(createRegisterDto.Password))
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı, e-posta ve şifre zorunludur.");
                return View(createRegisterDto);
            }

            if (createRegisterDto.Password != createRegisterDto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifre ve şifre onayı eşleşmiyor.");
                return View(createRegisterDto);
            }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createRegisterDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("http://localhost:5001/api/Registers", stringContent);
            var responseBody = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var signInResult = await _identityService.SignIn(new SignInDto
                {
                    Username = createRegisterDto.Username,
                    Password = createRegisterDto.Password
                });

                if (signInResult != null && signInResult.IsSuccess)
                {
                    TempData["RegisterSuccess"] = $"Hoş geldin {createRegisterDto.Name}! Hesabın oluşturuldu, giriş yaptın.";
                    return RedirectToAction("Index", "Default");
                }

                TempData["RegisterSuccess"] = "Kayıt başarılı. Lütfen giriş yapın.";
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var fail = JsonConvert.DeserializeObject<RegisterFailResponse>(responseBody);
                if (fail?.Errors != null && fail.Errors.Length > 0)
                {
                    foreach (var err in fail.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.");
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Kayıt sırasında bir hata oluştu.");
            }

            return View(createRegisterDto);
        }

        private class RegisterFailResponse
        {
            public bool Succeeded { get; set; }
            public string[] Errors { get; set; }
        }
    }
}
