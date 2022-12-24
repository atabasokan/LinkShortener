using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace LinkShortener.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public bool admin { get; set; }
        public int Urls { get; set; }

    }
}
