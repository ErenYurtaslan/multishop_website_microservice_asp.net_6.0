
using System.Globalization;
using System.Text.Json;

namespace MultiShop.WebUI.Services.StatisticServices.CatalogStatisticServices
{
    public class CatalogStatisticService : ICatalogStatisticService
    {
        private readonly HttpClient _httpClient;
        public CatalogStatisticService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<long> GetBrandCount()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetBrandCount");
            var values = await responseMessage.Content.ReadFromJsonAsync<long>();
            return values;
        }

        public async Task<long> GetCategoryCount()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetCategoryCount");
            var values = await responseMessage.Content.ReadFromJsonAsync<long>();
            return values;
        }

        public async Task<string> GetMaxPriceProductName()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetMaxPriceProductName");
            var values = await responseMessage.Content.ReadAsStringAsync();
            return values;
        }

        public async Task<string> GetMinPriceProductName()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetMinPriceProductName");
            var values = await responseMessage.Content.ReadAsStringAsync();
            return values;
        }

        public async Task<decimal> GetProductAvgPrice()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetProductAvgPrice");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return 0m;
            }

            var raw = (await responseMessage.Content.ReadAsStringAsync())?.Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return 0m;
            }

            // Endpoint occasionally returns non-JSON text; parse defensively.
            try
            {
                return JsonSerializer.Deserialize<decimal>(raw);
            }
            catch (JsonException)
            {
                if (decimal.TryParse(raw.Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
                {
                    return inv;
                }

                if (decimal.TryParse(raw.Trim('"'), NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var tr))
                {
                    return tr;
                }

                return 0m;
            }
        }

        public async Task<long> GetProductCount()
        {
            var responseMessage = await _httpClient.GetAsync("Statistics/GetProductCount");
            var values = await responseMessage.Content.ReadFromJsonAsync<long>();
            return values;
        }
    }
}
