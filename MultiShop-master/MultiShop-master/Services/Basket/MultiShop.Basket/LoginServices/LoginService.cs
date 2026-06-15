using System.Security.Claims;

namespace MultiShop.Basket.LoginServices
{
    public class LoginService : ILoginService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginService(IHttpContextAccessor contextAccessor)
        {
            _httpContextAccessor = contextAccessor;
        }

        public string GetUserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var id = user?.FindFirst("sub")?.Value ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException("JWT must include 'sub' (or nameidentifier) for basket.");
                }
                return id;
            }
        }
    }
}
