using MultiShop.DtoLayer.CargoDtos.CargoDetailDtos;

namespace MultiShop.WebUI.Services.CargoServices.CargoDetailServices
{
    public interface ICargoDetailService
    {
        Task<List<ResultCargoDetailDto>> GetAllCargoDetailsAsync();
        Task CreateCargoDetailAsync(CreateCargoDetailDto createCargoDetailDto);
    }
}
