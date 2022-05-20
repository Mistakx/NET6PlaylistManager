namespace SkyPlaylistManager.Models.DTOs.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserDetailsDTO
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = null!;
        
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("profilePhotoUrl")]
    public string ProfilePhotoUrl { get; set; } = null!;

    [BsonElement("favorites")]
    public List<PlaylistBasicDetailsDTO>? Favorites { get; set; }
}