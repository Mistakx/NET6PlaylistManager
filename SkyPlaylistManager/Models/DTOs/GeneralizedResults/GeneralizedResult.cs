using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.GeneralizedResults;

public abstract class GeneralizedResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("resultType")] public abstract string ResultType { get; set; }
    [BsonElement("platformId")] public string PlatformId { get; set; }
    [BsonElement("title")] public string Title { get; set; }
    [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }
    [BsonElement("creator")] public string Creator { get; set; }
    [BsonElement("playerFactoryName")] public string PlayerFactoryName { get; set; }
    [BsonElement("platformPlayerUrl")] public string? PlatformPlayerUrl { get; set; }

    protected GeneralizedResult(UnknownGeneralizedResultDto request)
    {
        PlatformId = request.PlatformId;
        Title =request.Title;
        ThumbnailUrl = request.ThumbnailUrl;
        Creator = request.Creator;
        PlayerFactoryName = request.PlayerFactoryName;
        PlatformPlayerUrl = request.PlatformPlayerUrl;
    }
}