using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.UserRequests;

namespace SkyPlaylistManager.Models.Database
{
    public class UserDocument
    {
        public UserDocument(UserSignupDto userSignup, string profilePhotoUrl)
        {
            Email = userSignup.Email;
            Password = BCrypt.Net.BCrypt.HashPassword(userSignup.Password);
            Name = userSignup.Name;
            Username = userSignup.Username;
            ProfilePhotoUrl = profilePhotoUrl;
            UserPlaylists = new List<ObjectId>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")] public string Email { get; set; }
        [BsonElement("username")] public string Username { get; set; }
        [BsonElement("password")] public string Password { get; set; }
        [BsonElement("name")] public string Name { get; set; }
        [BsonElement("profilePhotoUrl")] public string ProfilePhotoUrl { get; set; }
        [BsonElement("userPlaylists")] public List<ObjectId>? UserPlaylists { get; set; }
    }
}