using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.User {
    
    public class UserPlaylistsDto {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [JsonIgnore]
        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; } = null!;

        [BsonElement("userPlaylists")]
        public List<PlaylistBasicDetailsDTO>? UserPlaylists { get; set; }
    }
    
}
