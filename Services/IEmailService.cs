using System.Threading.Tasks;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendOrderConfirmationAsync(OrderConfirmationEmailDto dto);
    }
}