using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class DeletePlaylistDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string? SessionToken { get; set; }
}