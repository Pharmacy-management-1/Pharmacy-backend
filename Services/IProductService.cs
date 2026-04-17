using PharmacyApi.DTOs;

namespace PharmacyApi.Services;

public interface IProductService
{
   
        Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO productDto);
        Task<ProductResponseDTO> UpdateProductAsync(int id, ProductUpdateDTO productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductResponseDTO?> GetProductByIdAsync(int id);
        Task<(IEnumerable<ProductResponseDTO> Products, int TotalCount)> GetProductsAsync(ProductFilterDTO filter);
        Task<bool> UpdateInventoryAsync(InventoryUpdateDTO inventoryUpdate);
        Task<InventoryResponseDTO?> GetInventoryByProductIdAsync(int productId);
        Task<IEnumerable<InventoryResponseDTO>> GetLowStockProductsAsync(int threshold = 10);
        Task<bool> ReserveStockAsync(int productId, int quantity);
        Task<bool> ReleaseReservedStockAsync(int productId, int quantity);
        Task<bool> AnyProductRequiresPrescription(List<int> productIds);
}
