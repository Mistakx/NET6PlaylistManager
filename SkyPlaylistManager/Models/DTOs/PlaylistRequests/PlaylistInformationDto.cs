using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.UserRequests;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class PlaylistInformationDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")] public string Title { get; set; } = null!;

    [BsonElement("visibility")] public string Visibility { get; set; } = null!;

    [BsonElement("creationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? CreationDate { get; set; } = null!;

    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("thumbnailUrl")] public string? ThumbnailUrl { get; set; }

    [BsonElement("owner")] public UserBasicProfileDto? Owner { get; set; }
}