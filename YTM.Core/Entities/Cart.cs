using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YTM.Core.Entities
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("UserId")]
        public string UserId { get; set; } = null!;

        [BsonElement("Items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public decimal TotalAmount => Items.Sum(item => item.Quantity * item.Price);
    }
} 