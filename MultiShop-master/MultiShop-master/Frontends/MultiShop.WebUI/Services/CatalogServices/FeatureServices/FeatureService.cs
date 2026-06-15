using MultiShop.DtoLayer.CatalogDtos.FeatureDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CatalogServices.FeatureServices
{
    public class FeatureService:IFeatureService
    {
        private readonly HttpClient _httpClient;
        public FeatureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateFeatureAsync(CreateFeatureDto createFeatureDto)
        {
            await _httpClient.PostAsJsonAsync<CreateFeatureDto>("features", createFeatureDto);
        }
        public async Task DeleteFeatureAsync(string id)
        {
            await _httpClient.DeleteAsync("features?id=" + id);
        }
        public async Task<UpdateFeatureDto> GetByIdFeatureAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("features/" + id);
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;
            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonConvert.DeserializeObject<UpdateFeatureDto>(json);
        }
        public async Task<List<ResultFeatureDto>> GetAllFeatureAsync()
        {
            var responseMessage = await _httpClient.GetAsync("features");
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<ResultFeatureDto>();
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData)) return new List<ResultFeatureDto>();
            return JsonConvert.DeserializeObject<List<ResultFeatureDto>>(jsonData) ?? new List<ResultFeatureDto>();
        }
        public async Task UpdateFeatureAsync(UpdateFeatureDto updateFeatureDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateFeatureDto>("features", updateFeatureDto);
        }
    }
}
