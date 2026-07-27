using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Services
{
    public class MongoCounter
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
