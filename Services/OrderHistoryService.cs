using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly AppDbContext _context;

        public OrderHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderHistoryDto
            {
                OrderId = o.Id,
                OrderStatus = o.Status,
                TotalAmount = o.TotalAmount,
                OrderDate = o.CreatedAt,
                Items = o.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ImageUrl = oi.Product?.ImageUrl ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            });
        }

        public async Task<OrderHistoryDto?> GetOrderDetailAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return null;

            return new OrderHistoryDto
            {
                OrderId = order.Id,
                OrderStatus = order.Status,
                TotalAmount = order.TotalAmount,
                OrderDate = order.CreatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ImageUrl = oi.Product?.ImageUrl ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}