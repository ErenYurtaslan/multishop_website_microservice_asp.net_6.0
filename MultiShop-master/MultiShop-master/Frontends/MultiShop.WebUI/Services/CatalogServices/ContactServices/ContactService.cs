using MultiShop.DtoLayer.CatalogDtos.ContactDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CatalogServices.ContactServices
{
    public class ContactService:IContactService
    {
        private readonly HttpClient _httpClient;
        public ContactService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateContactAsync(CreateContactDto createContactDto)
        {
            await _httpClient.PostAsJsonAsync<CreateContactDto>("contacts", createContactDto);
        }
        public async Task DeleteContactAsync(string id)
        {
            await _httpClient.DeleteAsync("contacts?id=" + id);
        }
        public async Task<GetByIdContactDto> GetByIdContactAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("contacts/" + id);
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;
            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonConvert.DeserializeObject<GetByIdContactDto>(json);
        }
        public async Task<List<ResultContactDto>> GetAllContactAsync()
        {
            var responseMessage = await _httpClient.GetAsync("contacts");
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<ResultContactDto>();
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData)) return new List<ResultContactDto>();
            return JsonConvert.DeserializeObject<List<ResultContactDto>>(jsonData) ?? new List<ResultContactDto>();
        }
        public async Task UpdateContactAsync(UpdateContactDto updateContactDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateContactDto>("contacts", updateContactDto);
        }
    }
}
