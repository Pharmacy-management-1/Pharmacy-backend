using System.Threading.Tasks;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public interface IQuickReorderService
    {
        Task<QuickReorderResponseDto> ReorderAsync(int orderId, int userId);
    }
}