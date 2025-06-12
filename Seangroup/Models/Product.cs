using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seangroup.Models
{

    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }         // Название товара
        public string Description { get; set; }  // Описание товара
        public decimal Price { get; set; }       // Цена товара
        public string ImageUrl { get; set; }     // Ссылка на изображение
                                                 // Если нужны дополнительные характеристики, можно добавить коллекцию:
          public List<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
      
        public List<Review> Review { get; set; } = new();
    }
}
