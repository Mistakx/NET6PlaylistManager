using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.Database;

public class UnknownContentDocumentDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DatabaseId { get; set; }

    [BsonElement("platformName")] public string PlatformName { get; set; }
    [BsonElement("resultType")] public string ResultType { get; set; }
    [BsonElement("playerFactoryName")] public string PlayerFactoryName { get; set; }
    [BsonElement("title")] public string Title { get; set; }
    [BsonElement("platformId")] public string PlatformId { get; set; }
    [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }
    [BsonElement("creator")] public string Creator { get; set; }
    [BsonElement("platformPlayerUrl")] public string? PlatformPlayerUrl { get; set; }

    // Track
    [BsonElement("albumName")] public string? AlbumName { get; set; }

    // Podcast
    [BsonElement("url")] public string? Url { get; set; }
    [BsonElement("href")] public string? Href { get; set; }
    
    // Livestream
    [BsonElement("gameName")] public string? GameName { get; set; }

    // Radio
    [BsonElement("region")] public string? Region { get; set; }
    [BsonElement("website")] public string? Website { get; set; }

}