using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Order.V1;
using OrderDetailV1Dto =
    EcommerceAPI.DTOs.Order.V1.OrderDetailResponseDto;

using OrderDetailV2Dto =
    EcommerceAPI.DTOs.Order.V2.OrderDetailResponseDto;

using MoneyDto =
    EcommerceAPI.DTOs.Order.V2.MoneyDto;

namespace EcommerceAPI.Services
{
    public interface IOrderService
    {
        Task<CheckoutResponseDto> CheckoutAsync(int userId);
        Task<List<OrderPreviewResponseDto>> GetMyOrdersAsync(int userId);
        Task<OrderDetailV1Dto> GetOrderDetailV1Async(int userId, int orderId);
        Task<OrderDetailV2Dto> GetOrderDetailV2Async(int userId, int orderId);
        Task CancelOrderAsync(int userId, int orderId);
    }
}
