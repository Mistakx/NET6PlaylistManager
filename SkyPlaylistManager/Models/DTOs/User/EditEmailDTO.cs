namespace SkyPlaylistManager.Models.DTOs.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class EditEmailDTO
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string NewEmail { get; set; } = null!;
}