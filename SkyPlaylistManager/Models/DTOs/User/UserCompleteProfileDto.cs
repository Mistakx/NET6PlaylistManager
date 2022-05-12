using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.Playlist;

namespace SkyPlaylistManager.Models.DTOs.User {
    
    public class UserProfileDto {
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
        [BsonElement("username")]
        public string Username { get; set; } = null! ;

        [BsonElement("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; } = null!;

        [BsonElement("userPlaylists")]
        public List<PlaylistBasicDetailsDto>? UserPlaylists { get; set; }
    }
    
}
