using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(
        IDeliveryRepository deliveryRepository,
        IOrderRepository orderRepository,
        ILogger<DeliveryService> logger)
    {
        _deliveryRepository = deliveryRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<DeliveryDto> CreateDeliveryAsync(CreateDeliveryDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), dto.OrderId);

        if (order.Status != "Paid")
            throw new BusinessRuleException("Cannot create delivery for unpaid order");

        var existing = await _deliveryRepository.GetByOrderIdAsync(dto.OrderId);
        if (existing != null)
            throw new BusinessRuleException($"Delivery already exists for order {dto.OrderId}");

        var delivery = new Delivery(
            dto.OrderId,
            dto.Address,
            dto.PhoneNumber,
            dto.PostalCode,
            dto.Carrier);

        await _deliveryRepository.AddAsync(delivery);
        _logger.LogInformation("Delivery created for order {OrderId}", dto.OrderId);

        return await MapToDeliveryDto(delivery);
    }

    public async Task<DeliveryDto?> GetDeliveryByIdAsync(int id)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id);
        if (delivery == null || delivery.IsDeleted)
            return null;

        return await MapToDeliveryDto(delivery);
    }

    public async Task<DeliveryDto?> GetDeliveryByOrderIdAsync(int orderId)
    {
        var delivery = await _deliveryRepository.GetByOrderIdAsync(orderId);
        if (delivery == null || delivery.IsDeleted)
            return null;

        return await MapToDeliveryDto(delivery);
    }

    public async Task<DeliveryDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(dto.DeliveryId);
        if (delivery == null)
            throw new NotFoundException(nameof(Delivery), dto.DeliveryId);

        switch (dto.Status.ToLower())
        {
            case "processing":
                delivery.MarkAsProcessing();
                break;
            case "shipped":
                if (string.IsNullOrWhiteSpace(dto.TrackingNumber))
                    throw new BusinessRuleException("Tracking number is required for shipped status");
                delivery.MarkAsShipped(dto.TrackingNumber);
                break;
            case "delivered":
                delivery.MarkAsDelivered();
                break;
            case "failed":
                delivery.MarkAsFailed("Delivery failed");
                break;
            default:
                throw new BusinessRuleException($"Invalid status: {dto.Status}");
        }

        await _deliveryRepository.UpdateAsync(delivery);
        _logger.LogInformation("Delivery {DeliveryId} status updated to {Status}", dto.DeliveryId, dto.Status);

        return await MapToDeliveryDto(delivery);
    }

    public async Task<List<DeliveryDto>> GetPendingDeliveriesAsync()
    {
        var deliveries = await _deliveryRepository.GetPendingDeliveriesAsync();
        var result = new List<DeliveryDto>();

        foreach (var delivery in deliveries)
        {
            result.Add(await MapToDeliveryDto(delivery));
        }

        return result;
    }

    private async Task<DeliveryDto> MapToDeliveryDto(Delivery delivery)
    {
        return new DeliveryDto
        {
            Id = delivery.Id,
            OrderId = delivery.OrderId,
            Address = delivery.Address,
            PhoneNumber = delivery.PhoneNumber,
            PostalCode = delivery.PostalCode,
            Status = delivery.Status,
            EstimatedDeliveryDate = delivery.EstimatedDeliveryDate,
            ActualDeliveryDate = delivery.ActualDeliveryDate,
            TrackingNumber = delivery.TrackingNumber,
            Carrier = delivery.Carrier,
            CreatedAt = delivery.CreatedAt
        };
    }
}