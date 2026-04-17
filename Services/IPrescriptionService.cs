using PharmacyApi.Models.Domain;

namespace PharmacyApi.Services;

public interface IPrescriptionService
{
    Task<Prescription> UploadPrescriptionAsync(int userId, string filePath, int? orderId = null);
    Task<Prescription?> GetPrescriptionByIdAsync(int id);
    Task<IEnumerable<Prescription>> GetUserPrescriptionsAsync(int userId);
    Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync();
    Task<bool> ApprovePrescriptionAsync(int prescriptionId, bool approved, string? comments);
}