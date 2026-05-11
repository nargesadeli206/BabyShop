using BabyShop.Application.Dtos;
using BabyShop.Core.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IDeliveryService
{
    Task<DeliveryDto?> GetDeliveryDtoByIdAsync(int id);
    Task<DeliveryDto?> GetDeliveryDtoByOrderIdAsync(int orderId);
    Task<List<DeliveryDto>> GetPendingDeliveryDtosAsync();
    Task<DeliveryDto> CreateDeliveryAsync(Dtos.CreateDeliveryDto dto);
    Task<DeliveryDto> UpdateDeliveryStatusAsync(Dtos.UpdateDeliveryStatusDto dto);
    Task<bool> IsOrderPaidAsync(int orderId);
}