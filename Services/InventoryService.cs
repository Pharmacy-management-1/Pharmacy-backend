namespace PharmacyApi.Services;

public class InventoryService : IInventoryService
{
    // Dummy implementation – Team B will replace with real logic
    public async Task<bool> CheckStockAsync(int productId, int quantity)
    {
        // TEMPORARY: assume always in stock
        return await Task.FromResult(true);
    }

    public async Task DecrementStockAsync(int productId, int quantity)
    {
        // TEMPORARY: do nothing
        await Task.CompletedTask;
    }
}