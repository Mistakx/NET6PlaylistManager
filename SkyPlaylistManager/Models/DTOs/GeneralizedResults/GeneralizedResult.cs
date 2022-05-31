using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.GeneralizedResults;

public abstract class GeneralizedResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("resultType")] public abstract string ResultType { get; set; }
    [BsonElement("id")] private string PlatformId { get; set; }
    [BsonElement("title")] private string Title { get; set; }
    [BsonElement("thumbnailUrl")] private string ThumbnailUrl { get; set; }
    [BsonElement("creator")] private string Creator { get; set; }
    [BsonElement("playerFactoryName")] private string PlayerFactoryName { get; set; }
    [BsonElement("platformPlayerUrl")] private string? PlatformPlayerUrl { get; set; }

    protected GeneralizedResult(JsonObject request)
    {
        PlatformId = (string) request["id"]!;
        Title = (string) request["title"]!;
        ThumbnailUrl = (string) request["thumbnailUrl"]!;
        Creator = (string) request["creator"]!;
        PlayerFactoryName = (string) request["playerFactoryName"]!;
        PlatformPlayerUrl = (string) request["platformPlayerUrl"]!;
    }
}