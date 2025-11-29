using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ActionType { get; set; }

        public string PerformedBy { get; set; }

        public string? Image { get; set; } 

        public string ItemId { get; set; }

        public string ItemName { get; set; }

        public string ItemCategory { get; set; }

        public DateTime Timestamp { get; set; }

        public string OldData { get; set; }

        public string NewData { get; set; }

        public string? OrderId { get; set; }
    }
}
