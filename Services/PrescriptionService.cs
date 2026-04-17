using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.Models.Domain;

namespace PharmacyApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly AppDbContext _context;

    public PrescriptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription> UploadPrescriptionAsync(int userId, string filePath, int? orderId = null)
    {
        var prescription = new Prescription
        {
            UserId = userId,
            FilePath = filePath,
            UploadDate = DateTime.UtcNow,
            Status = "Pending",
            OrderId = orderId
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Prescription?> GetPrescriptionByIdAsync(int id)
    {
        return await _context.Prescriptions.FindAsync(id);
    }

    public async Task<IEnumerable<Prescription>> GetUserPrescriptionsAsync(int userId)
    {
        return await _context.Prescriptions
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync()
    {
        return await _context.Prescriptions
            .Where(p => p.Status == "Pending")
            .Include(p => p.User)
            .OrderBy(p => p.UploadDate)
            .ToListAsync();
    }

    public async Task<bool> ApprovePrescriptionAsync(int prescriptionId, bool approved, string? comments)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null) return false;

        prescription.Status = approved ? "Approved" : "Rejected";
        prescription.AdminComments = comments;

        await _context.SaveChangesAsync();
        return true;
    }
}