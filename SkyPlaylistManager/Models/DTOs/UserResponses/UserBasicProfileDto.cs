using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserBasicProfileDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("email")] public string Email { get; set; }
    [BsonElement("name")] public string Name { get; set; }
    [BsonElement("username")] public string Username { get; set; }
    [BsonElement("profilePhotoUrl")] public string ProfilePhotoUrl { get; set; }

    public UserBasicProfileDto(string email, string name, string username, string profilePhotoUrl)
    {
        Email = email;
        Name = name;
        Username = username;
        ProfilePhotoUrl = profilePhotoUrl;
    }
    
    public UserBasicProfileDto(string name, string username, string profilePhotoUrl)
    {
        Name = name;
        Username = username;
        ProfilePhotoUrl = profilePhotoUrl;
    }
}