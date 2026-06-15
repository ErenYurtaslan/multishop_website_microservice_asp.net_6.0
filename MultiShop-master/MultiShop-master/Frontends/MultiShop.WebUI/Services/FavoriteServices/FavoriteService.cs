using System.Net;
using MultiShop.DtoLayer.FavoriteDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.FavoriteServices
{
    public class FavoriteService : IFavoriteService
    {
        private readonly HttpClient _httpClient;

        public FavoriteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultUserFavoriteDto>> GetMyFavoritesAsync()
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync("favorites/MyFavorites");
                if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    return new List<ResultUserFavoriteDto>();
                }

                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return new List<ResultUserFavoriteDto>();
                }

                var values = JsonConvert.DeserializeObject<List<ResultUserFavoriteDto>>(jsonData);
                return values ?? new List<ResultUserFavoriteDto>();
            }
            catch
            {
                return new List<ResultUserFavoriteDto>();
            }
        }

        public async Task<int> GetMyFavoritesCountAsync()
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync("favorites/MyFavoritesCount");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return 0;
                }

                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return 0;
                }

                if (int.TryParse(jsonData, out var count))
                {
                    return count;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> IsFavoriteAsync(string productId)
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync($"favorites/IsFavorite/{productId}");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return false;
                }

                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return false;
                }

                return bool.TryParse(jsonData, out var result) && result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddFavoriteAsync(CreateUserFavoriteDto dto)
        {
            try
            {
                var responseMessage = await _httpClient.PostAsJsonAsync("favorites", dto);
                return responseMessage.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(int id)
        {
            try
            {
                var responseMessage = await _httpClient.DeleteAsync($"favorites/{id}");
                return responseMessage.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFavoriteByProductAsync(string productId)
        {
            try
            {
                var responseMessage = await _httpClient.DeleteAsync($"favorites/ByProduct/{productId}");
                return responseMessage.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
