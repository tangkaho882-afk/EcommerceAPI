using Asp.Versioning;
using EcommerceAPI.Extensions;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderDetailV1Dto =
    EcommerceAPI.DTOs.Order.V1.OrderDetailResponseDto;
using OrderDetailV2Dto =
    EcommerceAPI.DTOs.Order.V2.OrderDetailResponseDto;

namespace EcommerceAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("MyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.GetRequiredUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<OrderDetailV1Dto>> GetOrderDetailsV1(int id)
        {
            var userId = User.GetRequiredUserId();

            var order = await _orderService.GetOrderDetailV1Async(userId, id);

            return Ok(order);
        }

        [HttpGet("{id:int}")]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<OrderDetailV2Dto>> GetOrderDetailsV2(int id)
        {
            var userId = User.GetRequiredUserId();

            var order = await _orderService.GetOrderDetailV2Async(userId, id);

            return Ok(order);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.GetRequiredUserId();

            await _orderService.CancelOrderAsync(userId, id);
            return Ok(new { Message = "The order is cancelled successfully" });
        }
    }
}