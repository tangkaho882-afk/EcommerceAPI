using EcommerceAPI.Validations;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EcommerceAPI.DTOs.User
{
    public class RegisterDto
    {
        [Required]
        [NoConsecutiveSpaces]
        [StringLength(20,ErrorMessage ="User name is limited in 20 characters ")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(100, ErrorMessage = "User name is limited in 100 characters ")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\dA-Za-z]).{8,}$", ErrorMessage = "Make sure your password contains at least 8 characters, including numbers, uppercase and lowercase letters, and special characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "This is not consistent with the above password")]
        public string ConfirmedPassword { get; set; } = string.Empty;

        [Required]
        [EmailAddress] // 驗證必須符合 Email 格式
        public string Email { get; set; } = string.Empty;
    }
}
