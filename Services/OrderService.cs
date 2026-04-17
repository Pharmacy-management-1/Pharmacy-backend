using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs;
using PharmacyApi.Models.Domain;
using PharmacyApi.Models.Domin;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;
    private readonly ILoyaltyService _loyaltyService;
    private readonly IEmailService _emailService;

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
        bool needsPrescription = await AnyProductRequiresPrescription(productIds); // temporary helper

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
        var productNames = new Dictionary<int, string>();

        foreach (var item in orderDto.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
                throw new ArgumentException($"Product {item.ProductId} not found.");

            bool inStock = await _inventoryService.CheckStockAsync(item.ProductId, item.Quantity);
            if (!inStock)
                throw new InvalidOperationException($"Insufficient stock for product ID {item.ProductId}.");

            totalAmount += product.Price * item.Quantity;
            productNames[item.ProductId] = product.Name ?? $"Product {item.ProductId}";

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
        await _context.SaveChangesAsync();

        // 4. Decrement inventory
        foreach (var item in orderDto.Items)
        {
            await _inventoryService.DecrementStockAsync(item.ProductId, item.Quantity);
        }

        // 5. Add loyalty points (10 points per ₹100 spent)
        int points = (int)(totalAmount / 10);
        await _loyaltyService.AddPointsAsync(userId, points, "Order placed");

        // 6. Send email confirmation
        var emailDto = new OrderConfirmationEmailDto
        {
            OrderId = order.Id,
            CustomerEmail = (await _context.Users.FindAsync(userId))?.Email ?? "customer@example.com",
            CustomerName = (await _context.Users.FindAsync(userId))?.Username ?? "Customer",
            OrderDate = order.OrderDate,
            TotalAmount = totalAmount,
            Items = orderDto.Items.Select(i => new OrderItemSummaryDto
            {
                ProductId = i.ProductId,
                ProductName = productNames.GetValueOrDefault(i.ProductId, "Unknown"),
                Quantity = i.Quantity,
                UnitPrice = orderItems.First(oi => oi.ProductId == i.ProductId).UnitPrice
            }).ToList()
        };
        await _emailService.SendOrderConfirmationAsync(emailDto);

        return order;
    }

    // Temporary helper – replace with _productService.AnyProductRequiresPrescription when Team B adds it
    private async Task<bool> AnyProductRequiresPrescription(List<int> productIds)
    {
        foreach (var id in productIds)
        {
            var pro = await _productService.GetProductByIdAsync(id);
            if (pro?.RequiresPrescription == true)
                return true;
        }
        return false;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}