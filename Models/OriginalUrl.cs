using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LinkShortener.Models
{
    public class OriginalUrl
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string title { get; set; }
        public int Click { get; set; }

    }
}
