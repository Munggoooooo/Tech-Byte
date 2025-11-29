using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public string Username { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string Role { get; set; } // "Admin" or "Member"
        public string Contact { get; set; }
        public string Address { get; set; }

        public bool IsActivated { get; set; }
        public string ActivationCode { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpires { get; set; }

    }
}
