using EcommerceAPI.DTOs.Cart;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Services
{
    public interface ICartService
    {
        Task AddCartAsync(int userId,AddCartItemDto dto);
        Task<IEnumerable<CartPreviewDto>> GetCartPreviewAsync(int userId);
        Task UpdateCartAsync(int userId, int cartId, UpdateCartQtyDto dto);
        Task RemoveCartAsync(int userId, int cartId);
    }
}
