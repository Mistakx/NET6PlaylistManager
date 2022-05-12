namespace SkyPlaylistManager.Models.DTOs.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class EditPasswordDTO
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string OldPassword { get; set; } = null!;

    public string NewPassword { get; set; } = null!;
}