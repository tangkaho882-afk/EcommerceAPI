using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; } // 外鍵：指向 Order 表

        public int ProductId { get; set; } // 外鍵：指向 Product 表

        public string ProductName { get; set; } = string.Empty; // 下單時的商品名稱

        public int Quantity { get; set; } // 購買數量
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // 下單時的單價

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation Properties (導覽屬性)：讓 Entity Framework 知道它們的關聯
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
    }
}
