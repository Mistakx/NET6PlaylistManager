using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.UserRequests;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class PlaylistAndContentsDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("owner")]
    public UserBasicProfileDto? Owner { get; set; } 
        
    [BsonElement("visibility")]
    public string? Visibility { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("creationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? CreationDate { get; set; }

    [BsonElement("contents")]
    public List<UnknownGeneralizedResultDto>? Contents { get; set; }

    [BsonElement("sharedWith")]
    public List<UserBasicProfileDto>? SharedWith { get; set; }
}