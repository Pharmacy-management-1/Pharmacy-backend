namespace PharmacyApi.Services;

public interface IInventoryService
{
    Task<bool> CheckStockAsync(int productId, int quantity);
    Task DecrementStockAsync(int productId, int quantity);
}