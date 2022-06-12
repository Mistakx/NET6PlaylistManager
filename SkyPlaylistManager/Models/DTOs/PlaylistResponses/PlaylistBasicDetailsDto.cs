using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistBasicDetailsDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("title")] public string Title { get; set; }

    [BsonElement("visibility")] public string Visibility { get; set; }

    [BsonElement("creationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreationDate { get; set; }

    [BsonElement("description")] public string Description { get; set; }

    [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }
    [BsonElement("resultsAmount")] public int ResultsAmount { get; set; }
}