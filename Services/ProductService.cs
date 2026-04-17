using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs;
using PharmacyApi.Models.Domin;

namespace PharmacyApi.Services;

public class ProductService : IProductService
{

    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO productDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Check if category exists
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
                throw new ArgumentException($"Category with ID {productDto.CategoryId} not found");

            // Create product
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Manufacturer = productDto.Manufacturer,
                Price = productDto.Price,
                DosageForm = productDto.DosageForm,
                Strength = productDto.Strength,
                Packaging = productDto.Packaging,
                RequiresPrescription = productDto.RequiresPrescription,
                ImageUrl = productDto.ImageUrl,
                CategoryId = productDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Create inventory record
            var inventory = new Inventory
            {
                ProductId = product.Id,
                QuantityInStock = productDto.InitialStock,
                ReservedQuantity = 0,
                ReorderLevel = productDto.ReorderLevel,
                ReorderQuantity = productDto.ReorderQuantity,
                CostPrice = productDto.CostPrice,
                BatchNumber = productDto.BatchNumber,
                ExpiryDate = productDto.ExpiryDate,
                Location = productDto.Location,
                LastUpdated = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await MapToResponseDTO(product, inventory);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    public async Task<ProductResponseDTO> UpdateProductAsync(int id, ProductUpdateDTO productDto)
    {
        var product = await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new ArgumentException($"Product with ID {id} not found");

        if (productDto.Name != null)
            product.Name = productDto.Name;
        if (productDto.Description != null)
            product.Description = productDto.Description;
        if (productDto.Manufacturer != null)
            product.Manufacturer = productDto.Manufacturer;
        if (productDto.Price.HasValue)
            product.Price = productDto.Price.Value;
        if (productDto.DosageForm != null)
            product.DosageForm = productDto.DosageForm;
        if (productDto.Strength != null)
            product.Strength = productDto.Strength;
        if (productDto.Packaging != null)
            product.Packaging = productDto.Packaging;
        if (productDto.RequiresPrescription.HasValue)
            product.RequiresPrescription = productDto.RequiresPrescription.Value;
        if (productDto.ImageUrl != null)
            product.ImageUrl = productDto.ImageUrl;
        if (productDto.CategoryId.HasValue)
            product.CategoryId = productDto.CategoryId.Value;
        if (productDto.IsActive.HasValue)
            product.IsActive = productDto.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToResponseDTO(product, product.Inventory);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return false;

        // Soft delete - just mark as inactive
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductResponseDTO?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return null;

        return await MapToResponseDTO(product, product.Inventory);
    }

    public async Task<(IEnumerable<ProductResponseDTO> Products, int TotalCount)> GetProductsAsync(ProductFilterDTO filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Where(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchTerm) ||
                                     p.Manufacturer.ToLower().Contains(searchTerm) ||
                                     p.Description.ToLower().Contains(searchTerm));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.DosageForm))
        {
            query = query.Where(p => p.DosageForm == filter.DosageForm);
        }

        if (!string.IsNullOrWhiteSpace(filter.Packaging))
        {
            query = query.Where(p => p.Packaging != null && p.Packaging.Contains(filter.Packaging));
        }

        if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
        {
            query = query.Where(p => p.Manufacturer == filter.Manufacturer);
        }

        if (filter.RequiresPrescription.HasValue)
        {
            query = query.Where(p => p.RequiresPrescription == filter.RequiresPrescription.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (filter.InStock.HasValue && filter.InStock.Value)
        {
            query = query.Where(p => p.Inventory != null && p.Inventory.QuantityInStock > 0);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                _ => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        }
        else
        {
            query = query.OrderBy(p => p.Name);
        }

        // Apply pagination
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var productDTOs = new List<ProductResponseDTO>();
        foreach (var product in products)
        {
            productDTOs.Add(await MapToResponseDTO(product, product.Inventory));
        }

        return (productDTOs, totalCount);
    }

    public async Task<bool> UpdateInventoryAsync(InventoryUpdateDTO inventoryUpdate)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == inventoryUpdate.ProductId);

        if (inventory == null)
            throw new ArgumentException($"Inventory for product ID {inventoryUpdate.ProductId} not found");

        var newQuantity = inventory.QuantityInStock + inventoryUpdate.QuantityChange;
        if (newQuantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        inventory.QuantityInStock = newQuantity;
        inventory.LastUpdated = DateTime.UtcNow;

        // Log inventory change (you might want to create an InventoryTransaction table)
        _logger.LogInformation($"Inventory updated for product {inventoryUpdate.ProductId}: " +
                              $"Change={inventoryUpdate.QuantityChange}, " +
                              $"Reason={inventoryUpdate.Reason}, " +
                              $"Reference={inventoryUpdate.ReferenceNumber}");

        await _context.SaveChangesAsync();

        // Check if reorder is needed
        if (inventory.QuantityInStock <= inventory.ReorderLevel)
        {
            _logger.LogWarning($"Product {inventoryUpdate.ProductId} is below reorder level. " +
                              $"Current stock: {inventory.QuantityInStock}, " +
                              $"Reorder level: {inventory.ReorderLevel}");
            // Trigger reorder notification here
        }

        return true;
    }

    public async Task<InventoryResponseDTO?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
            return null;

        return MapToInventoryResponseDTO(inventory);
    }

    public async Task<IEnumerable<InventoryResponseDTO>> GetLowStockProductsAsync(int threshold = 10)
    {
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.QuantityInStock <= threshold && i.Product.IsActive)
            .ToListAsync();

        return inventories.Select(MapToInventoryResponseDTO);
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
            throw new ArgumentException($"Inventory for product ID {productId} not found");

        var availableStock = inventory.QuantityInStock - inventory.ReservedQuantity;
        if (availableStock < quantity)
            throw new InvalidOperationException($"Insufficient available stock. Available: {availableStock}, Requested: {quantity}");

        inventory.ReservedQuantity += quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseReservedStockAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
            throw new ArgumentException($"Inventory for product ID {productId} not found");

        if (inventory.ReservedQuantity < quantity)
            throw new InvalidOperationException($"Cannot release more than reserved. Reserved: {inventory.ReservedQuantity}, Requested release: {quantity}");

        inventory.ReservedQuantity -= quantity;
        inventory.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Helper methods
    private async Task<ProductResponseDTO> MapToResponseDTO(Product product, Inventory? inventory)
    {
        var category = await _context.Categories.FindAsync(product.CategoryId);

        return new ProductResponseDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Manufacturer = product.Manufacturer,
            Price = product.Price,
            DosageForm = product.DosageForm,
            Strength = product.Strength,
            Packaging = product.Packaging,
            RequiresPrescription = product.RequiresPrescription,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? string.Empty,
            StockQuantity = inventory?.QuantityInStock ?? 0,
            ReservedQuantity = inventory?.ReservedQuantity ?? 0,
            AvailableQuantity = (inventory?.QuantityInStock ?? 0) - (inventory?.ReservedQuantity ?? 0),
            ExpiryDate = inventory?.ExpiryDate,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }

    private InventoryResponseDTO MapToInventoryResponseDTO(Inventory inventory)
    {
        var availableQuantity = inventory.QuantityInStock - inventory.ReservedQuantity;
        var isExpired = inventory.ExpiryDate.HasValue && inventory.ExpiryDate.Value < DateTime.UtcNow;
        var isExpiringSoon = inventory.ExpiryDate.HasValue &&
                            inventory.ExpiryDate.Value > DateTime.UtcNow &&
                            inventory.ExpiryDate.Value < DateTime.UtcNow.AddDays(90);

        return new InventoryResponseDTO
        {
            ProductId = inventory.ProductId,
            ProductName = inventory.Product?.Name ?? string.Empty,
            QuantityInStock = inventory.QuantityInStock,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = availableQuantity,
            ReorderLevel = inventory.ReorderLevel,
            NeedsReorder = inventory.QuantityInStock <= inventory.ReorderLevel,
            BatchNumber = inventory.BatchNumber,
            ExpiryDate = inventory.ExpiryDate,
            IsExpiringSoon = isExpiringSoon,
            IsExpired = isExpired,
            LastUpdated = inventory.LastUpdated
        };
    }
}
    
