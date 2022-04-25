using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models.Database
{
    public class UserCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; } = null!;

        [BsonElement("userPlaylists")]
        public List<ObjectId>? UserPlaylists { get; set; }



        public UserCollection(NewUserDTO newUser)
        {
            Email = newUser.Email;
            Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            Name = newUser.Name;
            ProfilePhotoPath = "Path to default user profile photo";
            UserPlaylists = new List<ObjectId>();
        }
    }
}
