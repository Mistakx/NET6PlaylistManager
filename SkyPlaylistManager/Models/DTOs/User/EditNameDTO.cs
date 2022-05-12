namespace SkyPlaylistManager.Models.DTOs.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class EditNameDTO
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string NewName { get; set; } = null!;
}