using PharmacyApi.DTOs;

namespace PharmacyApi.Services;


    public interface ICategoryService
    {
        Task<CategoryResponseDTO> CreateCategoryAsync(CategoryCreateDTO categoryDto);
        Task<CategoryResponseDTO> UpdateCategoryAsync(int id, CategoryUpdateDTO categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<CategoryResponseDTO?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryResponseDTO>> GetAllCategoriesAsync(bool includeProducts = false);
        Task<IEnumerable<CategoryResponseDTO>> GetCategoryTreeAsync();
    }

