using EcommerceAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.User
{
    public class LoginDto
    {
        [Required]
        [StringLength(20,ErrorMessage = "User name is limited in 20 characters")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(100, ErrorMessage = "Password is limited in 100 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
