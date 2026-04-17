using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs;
using PharmacyApi.Models.Domin;

namespace PharmacyApi.Services;

    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponseDTO> CreateCategoryAsync(CategoryCreateDTO categoryDto)
        {
            // Validate parent category if provided
            if (categoryDto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == categoryDto.ParentCategoryId.Value);
                if (!parentExists)
                    throw new ArgumentException($"Parent category with ID {categoryDto.ParentCategoryId} not found");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentCategoryId = categoryDto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return await MapToResponseDTO(category);
        }

        public async Task<CategoryResponseDTO> UpdateCategoryAsync(int id, CategoryUpdateDTO categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new ArgumentException($"Category with ID {id} not found");

            if (categoryDto.Name != null)
                category.Name = categoryDto.Name;
            if (categoryDto.Description != null)
                category.Description = categoryDto.Description;
            if (categoryDto.ParentCategoryId.HasValue)
            {
                // Prevent circular reference
                if (categoryDto.ParentCategoryId.Value == id)
                    throw new InvalidOperationException("Category cannot be its own parent");

                var parentExists = await _context.Categories.AnyAsync(c => c.Id == categoryDto.ParentCategoryId.Value);
                if (!parentExists)
                    throw new ArgumentException($"Parent category with ID {categoryDto.ParentCategoryId} not found");

                category.ParentCategoryId = categoryDto.ParentCategoryId;
            }
            if (categoryDto.IsActive.HasValue)
                category.IsActive = categoryDto.IsActive.Value;

            await _context.SaveChangesAsync();

            return await MapToResponseDTO(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            // Check if category has products
            if (category.Products.Any())
                throw new InvalidOperationException("Cannot delete category that has products. Reassign or delete products first.");

            // Soft delete
            category.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CategoryResponseDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return null;

            return await MapToResponseDTO(category);
        }

        public async Task<IEnumerable<CategoryResponseDTO>> GetAllCategoriesAsync(bool includeProducts = false)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive);

            if (includeProducts)
                query = query.Include(c => c.Products);

            var categories = await query.ToListAsync();

            var result = new List<CategoryResponseDTO>();
            foreach (var category in categories)
            {
                result.Add(await MapToResponseDTO(category));
            }

            return result;
        }

        public async Task<IEnumerable<CategoryResponseDTO>> GetCategoryTreeAsync()
        {
            var allCategories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            return BuildCategoryTree(lookup, null);
        }

        private List<CategoryResponseDTO> BuildCategoryTree(ILookup<int?, Category> lookup, int? parentId)
        {
            var categories = lookup[parentId].ToList();
            var result = new List<CategoryResponseDTO>();

            foreach (var category in categories)
            {
                var dto = new CategoryResponseDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    SubCategories = BuildCategoryTree(lookup, category.Id)
                };
                result.Add(dto);
            }

            return result;
        }

        private async Task<CategoryResponseDTO> MapToResponseDTO(Category category)
        {
            var productCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id && p.IsActive);

            return new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                ProductCount = productCount,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }
    }

