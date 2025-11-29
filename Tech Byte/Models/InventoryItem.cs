using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class InventoryItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Image { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }
    }
}
