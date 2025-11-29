using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Tech_Byte.Models
{
    public class Purchase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string CustomerUserName { get; set; }

        public string CustomerEmail { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public List<CartItem> Items { get; set; } = new();

        public decimal Total { get; set; }

        public string OrderId { get; set; }
    }
}
