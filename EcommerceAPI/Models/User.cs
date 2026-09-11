using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class User
    {
        [Key] // 代表這是主鍵 (Primary Key)
        public int Id { get; set; }

        [Required] // 代表欄位不能為空 (NOT NULL)
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Customer"; // 預設角色為一般顧客

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 記錄註冊時間
    }
}