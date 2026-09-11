using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Order.V1;
using EcommerceAPI.DTOs.Order.V2;
using EcommerceAPI.Enums;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

using OrderDetailV1Dto =
    EcommerceAPI.DTOs.Order.V1.OrderDetailResponseDto;

using OrderItemDetailV1Dto =
    EcommerceAPI.DTOs.Order.V1.OrderItemDetailsResponseDto;

using OrderDetailV2Dto =
    EcommerceAPI.DTOs.Order.V2.OrderDetailResponseDto;

using OrderItemDetailV2Dto =
    EcommerceAPI.DTOs.Order.V2.OrderItemDetailsResponseDto;

using MoneyDto =
    EcommerceAPI.DTOs.Order.V2.MoneyDto;
using EcommerceAPI.DTOs.Checkout;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CheckoutResponseDto> CheckoutAsync(int userId)
        {
            List<CheckoutIssueDto> checkoutIssues = [];
            var cartItems = await _context.CartItems.Include(c => c.Product).Where(u => u.UserId == userId).ToListAsync();

            if (!cartItems.Any())
            {
                throw new BadRequestException("The cart is empty");
            }

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product == null)
                {
                    throw new Exception($"Product {cartItem.ProductId} referenced by the cart was not found.");
                }
                if (!cartItem.Product.IsActive)
                {
                    checkoutIssues.Add(new CheckoutIssueDto
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Reason = "ProductUnavailable"
                    });
                    continue;
                }
                if (cartItem.Product.Stock < cartItem.Quantity)
                {
                    checkoutIssues.Add(new CheckoutIssueDto
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Reason = "InsufficientStock"
                    });
                }
            }

            if (checkoutIssues.Any())
            {
                throw new CheckoutValidationException(checkoutIssues);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = 0
                };
                _context.Orders.Add(order);

                foreach (var cartItem in cartItems)
                {
                    var product = cartItem.Product!;
                    var orderItem = new OrderItem
                    {
                        Order = order,
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    order.TotalAmount += orderItem.Quantity * orderItem.UnitPrice;
                    product.Stock -= cartItem.Quantity;
                    _context.CartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("User {UserId} created the order {OrderId} successfully. Total amount is {TotalAmount}", 
                    userId, 
                    order.Id, 
                    order.TotalAmount);
                return new CheckoutResponseDto {
                    Message="Checkout successfully", 
                    OrderId= order.Id, 
                    TotalAmount=order.TotalAmount };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex,
                    "The row version changed during checkout. UserId: {UserId}",
                    userId);
                throw new ConcurrencyConflictException("The stock status has changed. Please try again.", ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderPreviewResponseDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await _context.Orders.Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .Select(o => new OrderPreviewResponseDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetailV1Dto> GetOrderDetailV1Async(
            int userId,
            int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Select(o => new OrderDetailV1Dto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    OrderItems = o.OrderItems
                        .Select(oi => new OrderItemDetailV1Dto
                        {
                            ItemId = oi.Id,
                            ItemName = oi.ProductName,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            SubTotal = oi.Quantity * oi.UnitPrice,
                            CreatedAt = oi.CreatedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new NotFoundException("The order was not found.");
            }

            return order;
        }

        public async Task<OrderDetailV2Dto> GetOrderDetailV2Async(int userId, int orderId)
        {
            var order = await _context.Orders.Where(o => o.Id == orderId && o.UserId == userId).Select(o => new OrderDetailV2Dto
            {
                OrderId = o.Id,
                TotalAmount = new MoneyDto
                {
                    Value = o.TotalAmount,
                    Currency = "HKD"
                },
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDetailV2Dto
                {
                    ItemId = oi.Id,
                    ItemName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubTotal = oi.Quantity * oi.UnitPrice,
                    CreatedAt = oi.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new NotFoundException("The order is not found");
            }

            return order;
        }

        public async Task CancelOrderAsync(int userId, int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.Where(o => o.Id == orderId && o.UserId == userId).Include(o => o.OrderItems).FirstOrDefaultAsync();
                if (order == null)
                {
                    throw new NotFoundException("The order is not found");
                }
                if (order.Status != OrderStatus.Pending)
                {
                    throw new BadRequestException("Only pending order can be cancelled");
                }
                var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                foreach (var orderItem in order.OrderItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Product {orderItem.ProductId} referenced by the order was not found.");
                    }
                    product.Stock += orderItem.Quantity;
                }
                order.Status = OrderStatus.Cancelled;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("User {UserId} cancels the order {OrderId} successfully.", 
                    userId, 
                    order.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new ConcurrencyConflictException("The order status has been changed, please try again",ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
