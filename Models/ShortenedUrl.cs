using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LinkShortener.Models
{
    public class ShortenedUrl
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public string ShortUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastDay { get; set;}
        public string User { get; set; }
        public int Click { get; set; }
        public string? SpeChar { get; set; }
    }
}