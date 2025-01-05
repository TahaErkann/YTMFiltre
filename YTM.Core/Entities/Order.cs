using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YTM.Core.Entities
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("UserId")]
        public string UserId { get; set; } = null!;

        [BsonElement("Items")]
        public List<OrderItem> Items { get; set; } = new();

        [BsonElement("TotalAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalAmount { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = OrderStatus.Pending;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
    }
} 