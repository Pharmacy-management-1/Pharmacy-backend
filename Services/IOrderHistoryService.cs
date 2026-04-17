using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public interface IOrderHistoryService
    {
        Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(int userId);
        Task<OrderHistoryDto?> GetOrderDetailAsync(int orderId, int userId);
    }
}