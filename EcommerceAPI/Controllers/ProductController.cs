using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Error;
using EcommerceAPI.DTOs.Product;
using EcommerceAPI.Extensions;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductResponseDto>> GetProductById([FromRoute] int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(product);
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<ProductResponseDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }
        [HttpGet("Admin")]
        [ProducesResponseType(
            typeof(IEnumerable<AdminProductResponseDto>),
            StatusCodes.Status200OK)]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<IEnumerable<AdminProductResponseDto>>> GetAllProductsForAdmin()
        {
            var products = await _productService.GetAllProductsForAdminAsync();
            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AdminProductResponseDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto),
            StatusCodes.Status400BadRequest)]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<AdminProductResponseDto>> PostProducts([FromBody] CreateProductDto productDto)
        {
            var userId = User.GetRequiredUserId();
            var product = await _productService.CreateProductAsync(userId, productDto);
            // 回傳 HTTP 201 Created，並把剛才生成好自動遞增 Id 的商品吐回去給前端
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.Id },
                product
            );
        }


        [HttpPut("{productId:int}")]
        [ProducesResponseType(typeof(AdminProductResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<AdminProductResponseDto>> UpdateProduct([FromRoute] int productId, [FromBody] UpdateProductDto newProductDto)
        {
            var userId = User.GetRequiredUserId();
            var product = await _productService.UpdateProductAsync(userId, productId, newProductDto);
            return Ok(product);
        }

        [HttpPatch("{productId:int}/disable")]
        [ProducesResponseType(
            StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DisableProduct([FromRoute] int productId)
        {
            var userId = User.GetRequiredUserId();
            await _productService.DisableProductAsync(userId, productId);
            return NoContent();
        }

        [HttpPatch("{productId:int}/enable")]
        [ProducesResponseType(
            StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), 
            StatusCodes.Status404NotFound)]
        [Authorize(Roles =AppRoles.Admin)]
        public async Task<IActionResult> EnableProduct([FromRoute] int productId)
        {
            var userId = User.GetRequiredUserId();
            await _productService.EnableProductAsync(userId, productId);
            return NoContent();
        }
    }
}
