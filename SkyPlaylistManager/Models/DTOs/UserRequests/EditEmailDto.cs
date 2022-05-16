namespace SkyPlaylistManager.Models.DTOs.UserRequests;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EditEmailDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string NewEmail { get; set; } = null!;
}