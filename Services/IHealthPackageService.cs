using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public interface IHealthPackageService
    {
        Task<IEnumerable<HealthPackageDto>> GetAllAsync();
        Task<HealthPackageDto?> GetByIdAsync(int id);
        Task<HealthPackageDto> CreateAsync(CreateHealthPackageDto dto);
        Task<HealthPackageDto?> UpdateAsync(int id, UpdateHealthPackageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}