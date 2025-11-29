using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class ContactMessage
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string FromUsername { get; set; }
        public string UserRole { get; set; }  // Admin or Member
        public string Email { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsRead { get; set; } = false;  // Tracks if admin has read the message
        public string? ReplyMessage { get; set; }   // Optional: store admin reply text
        public DateTime? ReplyTimestamp { get; set; } // When reply was sent

    }
}
