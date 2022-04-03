using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; } = null!;  
    }
}
