using PharmacyApi.DTOs;
using PharmacyApi.Models.Domain;

namespace PharmacyApi.Services;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(int userId, PlaceOrderDto orderDto);
    Task<Order?> GetOrderByIdAsync(int orderId);
}