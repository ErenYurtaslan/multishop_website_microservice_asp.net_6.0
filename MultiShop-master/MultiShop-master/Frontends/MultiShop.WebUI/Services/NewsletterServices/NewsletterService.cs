using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiShop.WebUI.Services.NewsletterServices
{
    public class NewsletterService : INewsletterService
    {
        private readonly HttpClient _httpClient;

        public NewsletterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NewsletterCallResult> GetStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Newsletter/Status");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new NewsletterCallResult { Outcome = NewsletterCallOutcome.Reauthenticate };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new NewsletterCallResult { Outcome = NewsletterCallOutcome.Error };
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new NewsletterCallResult { Outcome = NewsletterCallOutcome.Error };
                }

                var status = JsonConvert.DeserializeObject<NewsletterStatusDto>(json);
                return new NewsletterCallResult
                {
                    Outcome = NewsletterCallOutcome.Ok,
                    Status = status
                };
            }
            catch
            {
                return new NewsletterCallResult { Outcome = NewsletterCallOutcome.Error };
            }
        }

        public async Task<NewsletterCallOutcome> SubscribeAsync()
        {
            return await PostAsync("api/Newsletter/Subscribe");
        }

        public async Task<NewsletterCallOutcome> UnsubscribeAsync()
        {
            return await PostAsync("api/Newsletter/Unsubscribe");
        }

        private async Task<NewsletterCallOutcome> PostAsync(string relativePath)
        {
            try
            {
                var response = await _httpClient.PostAsync(relativePath,
                    new StringContent(string.Empty, Encoding.UTF8, "application/json"));

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return NewsletterCallOutcome.Reauthenticate;
                }

                return response.IsSuccessStatusCode
                    ? NewsletterCallOutcome.Ok
                    : NewsletterCallOutcome.Error;
            }
            catch
            {
                return NewsletterCallOutcome.Error;
            }
        }
    }
}
