using EcommerceAPI.DTOs.Product;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductResponseDto>> GetProductsAsync();
        Task<IEnumerable<AdminProductResponseDto>> GetAllProductsForAdminAsync();
        Task<AdminProductResponseDto> CreateProductAsync(int userId, CreateProductDto productDto);
        Task<AdminProductResponseDto> UpdateProductAsync(int userId, int productId, UpdateProductDto newProductDto);
        Task DisableProductAsync(int userId, int productId);
        Task EnableProductAsync(int userId, int productId);
    }
}
