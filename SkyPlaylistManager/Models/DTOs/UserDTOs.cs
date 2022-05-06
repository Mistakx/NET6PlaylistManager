using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;


namespace SkyPlaylistManager.Models.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }

    public class NewUserDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
    }


    public class UserPlaylistsDTO
    {
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



    public class UserBasicDetailsDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = null!;
        
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; } = null!;

    }


}
