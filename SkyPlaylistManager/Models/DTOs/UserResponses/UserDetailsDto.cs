using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;

namespace SkyPlaylistManager.Models.DTOs.UserRequests;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserDetailsDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")] public string Email { get; set; } = null!;

    [BsonElement("name")] public string Name { get; set; } = null!;

    [BsonElement("username")] public string Username { get; set; } = null!;

    [BsonElement("profilePhotoUrl")] public string ProfilePhotoUrl { get; set; } = null!;

    [BsonElement("favorites")] public List<PlaylistBasicDetailsDto>? Favorites { get; set; }
}