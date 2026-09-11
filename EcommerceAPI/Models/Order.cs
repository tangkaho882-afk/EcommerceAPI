using EcommerceAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // 外鍵：指向 User 表

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // 訂單總金額

        public OrderStatus Status { get; set; } = OrderStatus.Pending; // 預設狀態為待付款
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // 一對多關係：一張訂單會包含多個 OrderItem (用 List 表達)
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}