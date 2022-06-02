using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.UserRequests;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationDto
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

    [BsonElement("owner")] public UserBasicProfileDto Owner { get; set; }
}