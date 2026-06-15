using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Settings;
using System.Security.Claims;

namespace MultiShop.WebUI.Services.Concrete
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClientSettings _clientSettings;
        private readonly ServiceApiSettings _serviceApiSettings;

        public IdentityService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<ClientSettings> clientSettings, IOptions<ServiceApiSettings> serviceApiSettings)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _clientSettings = clientSettings.Value;
            _serviceApiSettings = serviceApiSettings.Value;
        }

        public async Task<bool> GetRefreshToken()
        {
            var discoveryEndPoint = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityServerUrl,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false
                }
            });

            var ctx = _httpContextAccessor.HttpContext;
            var refreshToken = await ctx.GetTokenAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectParameterNames.RefreshToken);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = await ctx.GetTokenAsync(
                    JwtBearerDefaults.AuthenticationScheme,
                    OpenIdConnectParameterNames.RefreshToken);
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = await ctx.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            RefreshTokenRequest refreshTokenRequest = new()
            {
                ClientId = _clientSettings.MultiShopManagerClient.ClientId,
                ClientSecret = _clientSettings.MultiShopManagerClient.ClientSecret,
                RefreshToken = refreshToken,
                Address = discoveryEndPoint.TokenEndpoint
            };

            var token = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest);
            if (token.IsError || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return false;
            }

            var authenticationToken = new List<AuthenticationToken>()
            {
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.AccessToken,
                    Value = token.AccessToken
                },
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.RefreshToken,
                    Value = token.RefreshToken
                },
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.ExpiresIn,
                    Value=DateTime.Now.AddSeconds(token.ExpiresIn).ToString()
                }
            };

            var result = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = result.Properties;
            if (properties == null || result.Principal == null)
            {
                return false;
            }
            properties.StoreTokens(authenticationToken);

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal, properties);

            return true;
        }

        public async Task<SignInResult> SignIn(SignInDto signInDto)
        {
            if (signInDto == null
                || string.IsNullOrWhiteSpace(signInDto.Username)
                || string.IsNullOrWhiteSpace(signInDto.Password))
            {
                return SignInResult.Fail("Kullanıcı adı ve şifre boş bırakılamaz.");
            }

            DiscoveryDocumentResponse discoveryEndPoint;
            try
            {
                discoveryEndPoint = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = _serviceApiSettings.IdentityServerUrl,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                });
            }
            catch
            {
                return SignInResult.Fail("Kimlik sunucusuna ulaşılamadı. Lütfen daha sonra tekrar deneyin.");
            }

            if (discoveryEndPoint == null || discoveryEndPoint.IsError)
            {
                return SignInResult.Fail("Kimlik sunucusu yapılandırması alınamadı.");
            }

            var passwordTokenRequest = new PasswordTokenRequest
            {
                ClientId = _clientSettings.MultiShopManagerClient.ClientId,
                ClientSecret = _clientSettings.MultiShopManagerClient.ClientSecret,
                UserName = signInDto.Username,
                Password = signInDto.Password,
                Address = discoveryEndPoint.TokenEndpoint,
                Scope = "openid profile email roles IdentityServerApi " +
                        "CatalogReadPermission CatalogFullPermission BasketFullPermission " +
                        "OcelotFullPermission CommentFullPermission PaymentFullPermission " +
                        "ImageFullPermission DiscountFullPermission OrderFullPermisson " +
                        "MessageFullPermission CargoFullPermission FavoriteFullPermission"
            };

            var token = await _httpClient.RequestPasswordTokenAsync(passwordTokenRequest);

            if (token == null || token.IsError || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return SignInResult.Fail("Kullanıcı adı veya şifre hatalı. Lütfen tekrar deneyin.");
            }

            var userInfoRequest = new UserInfoRequest
            {
                Token = token.AccessToken,
                Address = discoveryEndPoint.UserInfoEndpoint
            };

            var userValues = await _httpClient.GetUserInfoAsync(userInfoRequest);
            if (userValues == null || userValues.IsError || userValues.Claims == null)
            {
                return SignInResult.Fail("Kullanıcı bilgileri alınamadı.");
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userValues.Claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authenticationProperties = new AuthenticationProperties();

            authenticationProperties.StoreTokens(new List<AuthenticationToken>()
            {
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.AccessToken,
                    Value = token.AccessToken
                },
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.RefreshToken,
                    Value = token.RefreshToken ?? string.Empty
                },
                new AuthenticationToken
                {
                    Name=OpenIdConnectParameterNames.ExpiresIn,
                    Value=DateTime.Now.AddSeconds(token.ExpiresIn).ToString()
                }
            });

            authenticationProperties.IsPersistent = false;

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authenticationProperties);

            return SignInResult.Success();
        }
    }
}