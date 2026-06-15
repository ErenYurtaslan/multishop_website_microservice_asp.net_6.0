using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;

namespace MultiShop.WebUI.Services.OrderServices.OrderOderingServices
{
    public interface IOrderOderingService
    {
        Task<List<ResultOrderingByUserIdDto>> GetAllOrderingsAsync();
        Task<List<ResultOrderingByUserIdDto>> GetOrderingByUserId(string id);
        Task<int?> CreateOrderingAndGetIdAsync(CreateOrderingDto createOrderingDto);
        Task CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetailDto);
    }
}
