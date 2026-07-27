using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        [BsonIgnore]
        public decimal Subtotal => Price * Quantity;
    }
}
