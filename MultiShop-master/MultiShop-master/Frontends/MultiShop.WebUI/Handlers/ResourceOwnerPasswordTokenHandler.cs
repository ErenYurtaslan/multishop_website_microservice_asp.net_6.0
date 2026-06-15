using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiShop.WebUI.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MultiShop.WebUI.Handlers
{
    public class ResourceOwnerPasswordTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;

        public ResourceOwnerPasswordTokenHandler(IHttpContextAccessor httpContextAccessor, IIdentityService identityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            // HttpContent is single-use per HTTP send. If we 401-refresh-retry, the same message would post an empty body.
            string? bufferedBody = null;
            string? contentMediaType = null;
            if (request.Content != null)
            {
                bufferedBody = await request.Content.ReadAsStringAsync(cancellationToken);
                contentMediaType = request.Content.Headers.ContentType?.MediaType;
                request.Content.Dispose();
                var media = string.IsNullOrEmpty(contentMediaType) ? "application/json" : contentMediaType;
                request.Content = new StringContent(bufferedBody ?? "", Encoding.UTF8, media);
            }

            // Outside a normal request (or mis-scoped HttpClient), skip auth header — do not throw.
            string? accessToken = null;
            if (httpContext != null)
            {
                // Tokens live on the login cookie (ROPC), not the legacy "Bearer"-named cookie scheme.
                accessToken = await httpContext.GetTokenAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectParameterNames.AccessToken);
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = await httpContext.GetTokenAsync(
                        JwtBearerDefaults.AuthenticationScheme,
                        OpenIdConnectParameterNames.AccessToken);
                }
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                }

                // Token may be expired/removed in cookie props; try one refresh cycle before first send.
                if (string.IsNullOrEmpty(accessToken))
                {
                    var preflightRefresh = await _identityService.GetRefreshToken();
                    if (preflightRefresh)
                    {
                        accessToken = await httpContext.GetTokenAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            OpenIdConnectParameterNames.AccessToken);
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = await httpContext.GetTokenAsync(
                                JwtBearerDefaults.AuthenticationScheme,
                                OpenIdConnectParameterNames.AccessToken);
                        }
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && httpContext != null)
            {
                var tokenResponse = await _identityService.GetRefreshToken();

                if (tokenResponse)
                {
                    response.Dispose();
                    accessToken = await httpContext.GetTokenAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        OpenIdConnectParameterNames.AccessToken);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        accessToken = await httpContext.GetTokenAsync(
                            JwtBearerDefaults.AuthenticationScheme,
                            OpenIdConnectParameterNames.AccessToken);
                    }
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        accessToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                    }
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }

                    if (bufferedBody != null)
                    {
                        request.Content?.Dispose();
                        var media = string.IsNullOrEmpty(contentMediaType) ? "application/json" : contentMediaType;
                        request.Content = new StringContent(bufferedBody, Encoding.UTF8, media);
                    }

                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"[Basket/Auth] Unauthorized downstream call: {request.Method} {request.RequestUri}");
            }
            return response;
        }
    }
}
