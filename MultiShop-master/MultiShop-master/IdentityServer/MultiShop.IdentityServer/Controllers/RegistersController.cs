using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiShop.IdentityServer.Dtos;
using MultiShop.IdentityServer.Models;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace MultiShop.IdentityServer.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public RegistersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegister(UserRegisterDto userRegisterDto)
        {
            if (userRegisterDto == null
                || string.IsNullOrWhiteSpace(userRegisterDto.Username)
                || string.IsNullOrWhiteSpace(userRegisterDto.Email)
                || string.IsNullOrWhiteSpace(userRegisterDto.Password))
            {
                return BadRequest(new { Succeeded = false, Errors = new[] { "Kullanıcı adı, e-posta ve şifre zorunludur." } });
            }

            var values = new ApplicationUser()
            {
                UserName = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname
            };
            var result = await _userManager.CreateAsync(values, userRegisterDto.Password);
            if (result.Succeeded)
            {
                return Ok(new { Succeeded = true, Message = "Kullanıcı başarıyla eklendi" });
            }

            var errors = result.Errors?.Select(e => e.Description).ToArray() ?? new[] { "Bir hata oluştu, tekrar deneyiniz." };
            return BadRequest(new { Succeeded = false, Errors = errors });
        }
    }
}
