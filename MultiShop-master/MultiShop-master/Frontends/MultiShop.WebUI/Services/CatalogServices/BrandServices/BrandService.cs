using MultiShop.DtoLayer.CatalogDtos.BrandDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CatalogServices.BrandServices
{
    public class BrandService:IBrandService
    {
        private readonly HttpClient _httpClient;
        public BrandService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateBrandAsync(CreateBrandDto createBrandDto)
        {
            await _httpClient.PostAsJsonAsync<CreateBrandDto>("brands", createBrandDto);
        }
        public async Task DeleteBrandAsync(string id)
        {
            await _httpClient.DeleteAsync("brands?id=" + id);
        }
        public async Task<UpdateBrandDto> GetByIdBrandAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("brands/" + id);
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;
            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonConvert.DeserializeObject<UpdateBrandDto>(json);
        }
        public async Task<List<ResultBrandDto>> GetAllBrandAsync()
        {
            var responseMessage = await _httpClient.GetAsync("brands");
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<ResultBrandDto>();
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData)) return new List<ResultBrandDto>();
            return JsonConvert.DeserializeObject<List<ResultBrandDto>>(jsonData) ?? new List<ResultBrandDto>();
        }
        public async Task UpdateBrandAsync(UpdateBrandDto updateBrandDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateBrandDto>("brands", updateBrandDto);
        }
    }
}
