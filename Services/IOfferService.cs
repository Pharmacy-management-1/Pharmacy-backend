using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public interface IOfferService
    {
        Task<IEnumerable<SeasonalOfferDto>> GetAllOffersAsync();
        Task<IEnumerable<SeasonalOfferDto>> GetActiveOffersAsync();
        Task<SeasonalOfferDto?> GetOfferByIdAsync(int id);
        Task<SeasonalOfferDto> CreateOfferAsync(CreateSeasonalOfferDto dto);
        Task<SeasonalOfferDto?> UpdateOfferAsync(int id, UpdateSeasonalOfferDto dto);
        Task<bool> DeleteOfferAsync(int id);
    }
}