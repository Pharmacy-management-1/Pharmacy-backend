using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderHistoryController : ControllerBase
    {
        private readonly IOrderHistoryService _orderHistoryService;

        public OrderHistoryController(IOrderHistoryService orderHistoryService)
        {
            _orderHistoryService = orderHistoryService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Get all past orders for the authenticated user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetOrderHistory()
        {
            var orders = await _orderHistoryService.GetOrderHistoryAsync(GetUserId());
            return Ok(orders);
        }

        /// <summary>Get details of a specific order.</summary>
        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var order = await _orderHistoryService.GetOrderDetailAsync(orderId, GetUserId());
            if (order == null) return NotFound(new { message = "Order not found." });
            return Ok(order);
        }
    }
}