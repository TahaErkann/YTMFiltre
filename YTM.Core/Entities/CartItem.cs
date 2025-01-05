using MongoDB.Bson.Serialization.Attributes;

namespace YTM.Core.Entities
{
    public class CartItem
    {
        [BsonElement("ProductId")]
        public string ProductId { get; set; } = null!;

        [BsonElement("ProductName")]
        public string ProductName { get; set; } = null!;

        [BsonElement("ImageUrl")]
        public string? ImageUrl { get; set; }

        [BsonElement("Size")]
        public int Size { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("Price")]
        [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
        public decimal Price { get; set; }
    }
}