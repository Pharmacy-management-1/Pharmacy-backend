using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs;
using PharmacyApi.Models.Domain;
using PharmacyApi.Services; // Assume these interfaces exist

namespace PharmacyApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IProductService _productService;      // Team B
    private readonly IInventoryService _inventoryService;  // Team B
    private readonly ILoyaltyService _loyaltyService;      // Team A
    private readonly IEmailService _emailService;          // Team D

    public OrderService(
        AppDbContext context,
        IProductService productService,
        IInventoryService inventoryService,
        ILoyaltyService loyaltyService,
        IEmailService emailService)
    {
        _context = context;
        _productService = productService;
        _inventoryService = inventoryService;
        _loyaltyService = loyaltyService;
        _emailService = emailService;
    }

    public async Task<Order> PlaceOrderAsync(int userId, PlaceOrderDto orderDto)
    {
        // 1. Validate prescription requirement
        var productIds = orderDto.Items.Select(i => i.ProductId).ToList();
        bool needsPrescription = await _productService.AnyProductRequiresPrescription(productIds);

        if (needsPrescription && orderDto.PrescriptionId == null)
            throw new InvalidOperationException("Prescription is required for this order.");

        if (orderDto.PrescriptionId != null)
        {
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == orderDto.PrescriptionId && p.UserId == userId);
            if (prescription == null)
                throw new ArgumentException("Invalid prescription.");
            if (prescription.Status != "Approved")
                throw new InvalidOperationException("Prescription not approved yet.");
        }

        // 2. Check stock & calculate total
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in orderDto.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
                throw new ArgumentException($"Product {item.ProductId} not found.");

            bool inStock = await _inventoryService.CheckStockAsync(item.ProductId, item.Quantity);
            if (!inStock)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}.");

            totalAmount += product.Price * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        // 3. Create order
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = "Confirmed",
            PrescriptionId = orderDto.PrescriptionId,
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Save to get Order.Id

        // 4. Decrement inventory (Team B)
        foreach (var item in orderDto.Items)
        {
            await _inventoryService.DecrementStockAsync(item.ProductId, item.Quantity);
        }

        // 5. Add loyalty points (Team A) - e.g., 10 points per 100 spent
        int points = (int)(totalAmount / 10); // 10 points per ₹100
        await _loyaltyService.AddPointsAsync(userId, points);

        // 6. Send email confirmation (Team D)
        await _emailService.SendOrderConfirmationEmailAsync(order.Id, userId);

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}