using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class AccountLog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Action { get; set; }             // Create, Edit, Delete
        public string PerformedBy { get; set; }        // Admin username
        public string AccountUsername { get; set; }     // Affected account
        public DateTime Timestamp { get; set; }

        public string Details { get; set; }             // JSON serialized User object
    }
}
