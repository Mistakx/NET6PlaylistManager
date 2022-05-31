using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.GeneralizedResults;

public class UnknownGeneralizedResultDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string DatabaseId { get; set; }

    [BsonElement("resultType")] public string ResultType { get; set; }
    [BsonElement("playerFactoryName")] public string PlayerFactoryName { get; set; }
    [BsonElement("title")] public string Title { get; set; }
    [BsonElement("id")] public string PlatformId { get; set; }
    [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }
    [BsonElement("creator")] public string Creator { get; set; }
    [BsonElement("platformPlayerUrl")] public string? PlatformPlayerUrl { get; set; }
    [BsonElement("albumName")] public string? AlbumName { get; set; }
    [BsonElement("gameName")] public string? GameName { get; set; }
}