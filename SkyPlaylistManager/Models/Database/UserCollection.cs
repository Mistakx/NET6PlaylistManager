using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Models.Database
{
    public class UserCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = null!;
        [BsonElement("username")]
        public string Username { get; set; }= null!;

        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("profilePhotoUrl")]
        public string ProfilePhotoUrl { get; set; } = null!;

        [BsonElement("userPlaylists")]
        public List<ObjectId>? UserPlaylists { get; set; }
        [BsonElement("favorites")]
        public List<ObjectId>? Favorites { get; set; }



        public UserCollection(NewUserDto newUser)
        {
            Email = newUser.Email;
            Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            Name = newUser.Name;
            Username = newUser.Username;
            ProfilePhotoUrl = "Path to default user profile photo"; // TODO Change default profile path
            UserPlaylists = new List<ObjectId>();
            Favorites = new List<ObjectId>();
        }
    }
}
