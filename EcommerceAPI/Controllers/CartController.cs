using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Error;
using EcommerceAPI.Extensions;
using EcommerceAPI.Filters;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CartController(IOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    
    [HttpPost]
    public async Task<IActionResult> AddCart([FromBody] AddCartItemDto dto)
    {
        // 1. Get userId from JWT
        var userId = User.GetRequiredUserId();

        await _cartService.AddCartAsync(userId, dto);

        return Created();
    }

    [HttpGet]
    [ProducesResponseType(
    typeof(IEnumerable<CartPreviewDto>),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CartPreviewDto>>> CartPreview()
    {
        var userId = User.GetRequiredUserId();

        var cartPreview = await _cartService.GetCartPreviewAsync(userId);

        return Ok(cartPreview);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
    typeof(ErrorResponseDto),
    StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQty([FromRoute] int id, [FromBody] UpdateCartQtyDto dto)
    {
        var userId = User.GetRequiredUserId();

        await _cartService.UpdateCartAsync(userId, id, dto);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
    typeof(ErrorResponseDto),
    StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCart([FromRoute] int id)
    {
        var userId = User.GetRequiredUserId();

        await _cartService.RemoveCartAsync(userId, id);

        return NoContent();
    }

    [ServiceFilter(typeof(CheckoutAuditFilter))]
    [HttpPost("Checkout")]
    [EnableRateLimiting(RateLimitPolicies.Checkout)]
    [Produces("application/json")]

    [ProducesResponseType(
    typeof(CheckoutResponseDto),
    StatusCodes.Status200OK)]

    [ProducesResponseType(
    typeof(ErrorResponseDto),
    StatusCodes.Status400BadRequest)]

    [ProducesResponseType(
    typeof(ErrorResponseDto),
    StatusCodes.Status409Conflict)]

    [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]

    [ProducesResponseType(
    StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout()
    {
        var userId = User.GetRequiredUserId();

        var checkoutResponse = await _orderService.CheckoutAsync(userId);

        return Ok(checkoutResponse);
    }
}