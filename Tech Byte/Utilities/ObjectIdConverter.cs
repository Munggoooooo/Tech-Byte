using MongoDB.Bson;
using Newtonsoft.Json;
using System;

namespace Tech_Byte.Utilities
{
    // Custom converter for MongoDB ObjectId
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        // Serialize ObjectId to string
        public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        // Deserialize string to ObjectId
        public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return ObjectId.Parse(reader.Value.ToString());
        }
    }
}
