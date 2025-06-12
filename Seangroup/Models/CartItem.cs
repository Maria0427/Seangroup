using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Seangroup.Areas.Identity.Data;

namespace Seangroup.Models
{
    public class CartItem 
    {

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; } // Тип string (как в AspNetUsers.Id)
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Навигационные свойства
        public Product Product { get; set; }
        public ApplicationUser User { get; set; } // Ссылка на IdentityUser

    }
}
