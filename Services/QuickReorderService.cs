using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.Models.Domain;
using PharmacyApi.Models.Domin;
using PharmacyApi.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyApi.Services;

public class QuickReorderService : IQuickReorderService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public QuickReorderService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<QuickReorderResponseDto> ReorderAsync(int orderId, int userId)
    {
        var originalOrder = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p!.Inventory)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (originalOrder == null)
            return new QuickReorderResponseDto { Success = false, Message = "Order not found." };

        var outOfStock = new List<string>();
        var validItems = new List<OrderItem>();

        foreach (var item in originalOrder.OrderItems)
        {
            var inventory = item.Product?.Inventory;
            if (inventory == null || inventory.StockQuantity < item.Quantity)
                outOfStock.Add(item.Product?.Name ?? $"ProductId:{item.ProductId}");
            else
                validItems.Add(item);
        }

        if (!validItems.Any())
            return new QuickReorderResponseDto
            {
                Success = false,
                Message = "None of the items are available in stock.",
                OutOfStockItems = outOfStock
            };

        var newOrder = new Order
        {
            UserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = validItems.Sum(i => i.UnitPrice * i.Quantity),
            OrderItems = validItems.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.Orders.Add(newOrder);

        // Decrement inventory for valid items
        foreach (var item in validItems)
        {
            var inv = item.Product!.Inventory!;
            inv.StockQuantity -= item.Quantity;
            inv.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send confirmation email
        if (originalOrder.User?.Email != null)
        {
            await _emailService.SendOrderConfirmationAsync(new OrderConfirmationEmailDto
            {
                OrderId = newOrder.Id,
                CustomerEmail = originalOrder.User.Email,
                CustomerName = originalOrder.User.Name ?? originalOrder.User.Email,
                OrderDate = newOrder.CreatedAt,
                TotalAmount = newOrder.TotalAmount,
                Items = validItems.Select(i => new OrderItemSummaryDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    ImageUrl = i.Product?.ImageUrl ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }

        return new QuickReorderResponseDto
        {
            Success = true,
            Message = outOfStock.Any()
                ? $"Order placed with {validItems.Count} item(s). Some items were out of stock."
                : "Order placed successfully.",
            NewOrderId = newOrder.Id,
            OutOfStockItems = outOfStock
        };
    }
}