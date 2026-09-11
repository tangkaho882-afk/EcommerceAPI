using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class CartService:ICartService
    {
        private readonly AppDbContext _context;
        public CartService(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddCartAsync(int userId, AddCartItemDto dto)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }
            if (!product.IsActive)
            {
                throw new BadRequestException(
                    "This product is no longer available.");
            }
            // 3. Check existing cart item
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == dto.ProductId && c.UserId == userId);
            // 4. Check stock
            var newQuantity = cartItem == null ? dto.Quantity : cartItem.Quantity + dto.Quantity;

            if (newQuantity > product.Stock)
            {
                throw new BadRequestException("Out of Stock");
            }

            // 5. If exists, update quantity
            if (cartItem != null)
            {
                cartItem.Quantity = newQuantity;
            }
            // 6. If not exists, add new cart item
            else
            {
                var addCartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                _context.CartItems.Add(addCartItem);
            }
            // 7. SaveChanges
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<CartPreviewDto>> GetCartPreviewAsync(int userId)
        {
            return await _context.CartItems.Where(c => c.UserId == userId)
            .Select(c => new CartPreviewDto
            {
                CartItemId = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product!.Name,
                Price = c.Product!.Price,
                Quantity = c.Quantity,
                SubTotal = c.Product.Price * c.Quantity,
                IsActive = c.Product.IsActive
            }).ToListAsync();
        }
        public async Task UpdateCartAsync(int userId, int cartId, UpdateCartQtyDto dto)
        {
            var cartItem = await _context.CartItems.Include(c=>c.Product).FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
            if (cartItem == null)
            {
                throw new NotFoundException("The cart item was not found.");
            }
            if (cartItem.Product == null)
            {
                throw new Exception(
                    $"Product {cartItem.ProductId} referenced by the cart was not found.");
            }
            if (!cartItem.Product.IsActive)
            {
                throw new BadRequestException(
                    "This product is no longer available.");
            }
            //handle the stock insufficient
            if (dto.Quantity > cartItem.Product.Stock)
            {
                throw new BadRequestException("Out of Stock.");
            }
            cartItem.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
        }
        public async Task RemoveCartAsync(int userId, int cartId)
        {
            //2.用 id + userId 找 CartItem
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
            //3.找唔到 → 404
            if (cartItem == null)
            {
                throw new NotFoundException("Cart Item was not found.");
            }
            //4.刪除 CartItem
            _context.CartItems.Remove(cartItem);
            //5.儲存
            await _context.SaveChangesAsync();
        }
    }
}
