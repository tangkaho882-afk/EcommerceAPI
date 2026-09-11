using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Product;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceAPI.Services
{
    public class ProductService:IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;
        public ProductService(AppDbContext context, ILogger<ProductService>logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ProductResponseDto> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p=>p.Id==productId && p.IsActive);
            if(product == null)
            {
                throw new NotFoundException($"The product {productId} was not found.");
            }
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            };
        }
        public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync()
        {
            return await _context.Products
                .Where(p=>p.IsActive)
                .Select(p=>new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<AdminProductResponseDto>> GetAllProductsForAdminAsync()
        {
            return await _context.Products
                .Select(p=>new AdminProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }
        public async Task<AdminProductResponseDto> CreateProductAsync(int userId, CreateProductDto productDto)
        {
            // 將前端傳過來的產品資料加進 EF Core 的追蹤隊列
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product);
            // 正式非同步寫入真實的資料庫裡
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} created the product {ProductId} successfully", userId, product.Id);

            return new AdminProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive
            };
        }
        public async Task<AdminProductResponseDto> UpdateProductAsync(int userId, int productId, UpdateProductDto newProductDto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new NotFoundException($"Product {productId} was not found.");
            }
            //for logging
            var oldProduct = new UpdateProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            };
            //update
            product.Name = newProductDto.Name;
            product.Description = newProductDto.Description;
            product.Price = newProductDto.Price;
            product.Stock = newProductDto.Stock;

            await _context.SaveChangesAsync();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("User {UserId} updated the product {ProductId} successfully. Old: {OldProduct}, New: {NewProduct}.",
                    userId,
                    productId,
                    JsonSerializer.Serialize(oldProduct),
                    JsonSerializer.Serialize(newProductDto));
            }
            return new AdminProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive
            };
        }
        public async Task DisableProductAsync(int userId, int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId) ??
                throw new NotFoundException($"The product {productId} was not found.");

            if (!product.IsActive) return;

            product.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} deactivated the product {ProductId}:{ProductName} successfully",
                userId,
                productId,
                product.Name);
        }
        public async Task EnableProductAsync(int userId, int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId) ??
                throw new NotFoundException($"The product {productId} was not found.");

            if (product.IsActive) return;
            product.IsActive = true;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} activated the product {ProductId}:{ProductName} successfully",
                userId,
                productId,
                product.Name);
        }
    }
}
