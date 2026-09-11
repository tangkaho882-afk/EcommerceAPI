using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt"; 
        [Required]
        [MinLength(32)]
        public string Key { get; set; } = string.Empty;
        [Required]
        public string Issuer { get; set; } = string.Empty;
        [Required]
        public string Audience { get; set; } = string.Empty;
    }
}
