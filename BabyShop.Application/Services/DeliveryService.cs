using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Dtos;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;

namespace BabyShop.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;

    public DeliveryService(IDeliveryRepository deliveryRepository)
    {
        _deliveryRepository = deliveryRepository;
    }

    public async Task<DeliveryDto?> GetDeliveryDtoByIdAsync(int id)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id);
        if (delivery == null) return null;

        return MapToDto(delivery);
    }

    public async Task<DeliveryDto?> GetDeliveryDtoByOrderIdAsync(int orderId)
    {
        var delivery = await _deliveryRepository.GetByOrderIdAsync(orderId);
        if (delivery == null) return null;

        return MapToDto(delivery);
    }

    public async Task<List<DeliveryDto>> GetPendingDeliveryDtosAsync()
    {
        var deliveries = await _deliveryRepository.GetPendingDeliveriesAsync();
        return deliveries.Select(MapToDto).ToList();
    }

    public async Task<DeliveryDto> CreateDeliveryAsync(Dtos.CreateDeliveryDto dto)
    {
        // ✅ درست: استفاده از سازنده کلاس Delivery
        var delivery = new Delivery(
            dto.OrderId,
            dto.Address,
            dto.PhoneNumber,
            dto.PostalCode,
            dto.Carrier
        );

        var createdDelivery = await _deliveryRepository.CreateDeliveryAsync(delivery);
        return MapToDto(createdDelivery);
    }

    public async Task<DeliveryDto> UpdateDeliveryStatusAsync(Dtos.UpdateDeliveryStatusDto dto)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(dto.DeliveryId);
        if (delivery == null)
            throw new Exception("Delivery not found");

        // ✅ درست: استفاده از متدهای Business Logic کلاس Delivery
        if (dto.Status == "Shipped")
        {
            delivery.MarkAsShipped(dto.TrackingNumber ?? string.Empty);
        }
        else if (dto.Status == "Delivered")
        {
            delivery.MarkAsDelivered();
        }
        else if (dto.Status == "Processing")
        {
            delivery.MarkAsProcessing();
        }
        else if (dto.Status == "Failed")
        {
            delivery.MarkAsFailed("Unknown reason");
        }

        await _deliveryRepository.UpdateAsync(delivery);
        return MapToDto(delivery);
    }

    public async Task<bool> IsOrderPaidAsync(int orderId)
    {
        return await _deliveryRepository.IsOrderPaidAsync(orderId);
    }

    // متد کمکی برای تبدیل Entity به DTO
    private DeliveryDto MapToDto(Delivery delivery)
    {
        return new DeliveryDto
        {
            Id = delivery.Id,
            OrderId = delivery.OrderId,
            Address = delivery.Address,
            PhoneNumber = delivery.PhoneNumber,
            PostalCode = delivery.PostalCode,
            Status = delivery.Status,
            TrackingNumber = delivery.TrackingNumber,
            Carrier = delivery.Carrier,
            EstimatedDeliveryDate = delivery.EstimatedDeliveryDate,
            ActualDeliveryDate = delivery.ActualDeliveryDate,
            CreatedAt = delivery.CreatedAt
        };
    }
}