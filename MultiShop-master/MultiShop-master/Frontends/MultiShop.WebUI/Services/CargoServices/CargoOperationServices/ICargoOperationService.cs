using MultiShop.DtoLayer.CargoDtos.CargoOperationDtos;

namespace MultiShop.WebUI.Services.CargoServices.CargoOperationServices
{
    public interface ICargoOperationService
    {
        Task<List<ResultCargoOperationDto>> GetAllCargoOperationsAsync();
        Task CreateCargoOperationAsync(CreateCargoOperationDto createCargoOperationDto);
    }
}
