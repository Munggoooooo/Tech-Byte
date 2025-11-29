using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tech_Byte.Models
{
    public class OrderSequence
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Date { get; set; } = DateTime.UtcNow.ToString("yyyyMMdd"); // e.g. 20251018

        public int Sequence { get; set; } = 0;
    }
}
