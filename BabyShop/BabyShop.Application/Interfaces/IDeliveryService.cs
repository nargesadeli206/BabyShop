using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryDto> CreateDeliveryAsync(CreateDeliveryDto dto);
    Task<DeliveryDto?> GetDeliveryByIdAsync(int id);
    Task<DeliveryDto?> GetDeliveryByOrderIdAsync(int orderId);
    Task<DeliveryDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto);
    Task<List<DeliveryDto>> GetPendingDeliveriesAsync();
}