using Seangroup.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seangroup.Models
{
    public class Order
    {
        [NotMapped]
        public decimal Total { get; set; }
        public int Id { get; set; }
        public string UserId { get; set; } // Если используется аутентификация
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Оформлен";
        public string DeliveryMethod { get; set; } // "Самовывоз" или "Доставка"
        public string DeliveryAddress { get; set; } // Необязательно при самовывозе
        public string? PaymentId { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
