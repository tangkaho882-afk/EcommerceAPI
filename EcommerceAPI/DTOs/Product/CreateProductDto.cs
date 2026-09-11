using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(20,ErrorMessage="Word count is limited to 20 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(100,ErrorMessage ="Word count is limited to 100 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01,99999,ErrorMessage = "Price range limited to 0.01~99999")]
        public decimal Price { get; set; } = 0.01m;

        [Required(ErrorMessage = "Stock is required")]
        [Range(1, 100, ErrorMessage = "Price range limited to 1~100")]
        public int Stock { get; set; } = 1;
    }
}
