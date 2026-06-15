using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultiShop.IdentityServer.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiShop.IdentityServer.Controllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NewsletterController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }

        [HttpGet("Status")]
        public async Task<IActionResult> Status()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            return Ok(new
            {
                IsSubscribed = user.IsSubscribed,
                Email = user.Email
            });
        }

        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!user.IsSubscribed)
            {
                user.IsSubscribed = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors?.Select(e => e.Description).ToArray()
                                 ?? new[] { "Abonelik güncellenemedi." };
                    return BadRequest(new { Succeeded = false, Errors = errors });
                }
            }

            return Ok(new
            {
                Succeeded = true,
                Message = "Bültenimize başarıyla abone oldun.",
                Email = user.Email,
                IsSubscribed = true
            });
        }

        [HttpPost("Unsubscribe")]
        public async Task<IActionResult> Unsubscribe()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.IsSubscribed)
            {
                user.IsSubscribed = false;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors?.Select(e => e.Description).ToArray()
                                 ?? new[] { "Abonelik güncellenemedi." };
                    return BadRequest(new { Succeeded = false, Errors = errors });
                }
            }

            return Ok(new
            {
                Succeeded = true,
                Message = "Bülten aboneliğinden çıktın.",
                Email = user.Email,
                IsSubscribed = false
            });
        }
    }
}
