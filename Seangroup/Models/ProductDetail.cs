namespace Seangroup.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string AttributeName { get; set; }    // Например, "Цвет"
        public string AttributeValue { get; set; }   // Например, "Красный"
    }
}
