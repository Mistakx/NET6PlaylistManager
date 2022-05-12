using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Models.DTOs.Playlist;

public class PlaylistAndContentsDto
{
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("owner")]
    public UserBasicDetailsDto? Owner { get; set; } 
        
    [BsonElement("visibility")]
    public string? Visibility { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("creationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? CreationDate { get; set; }

    [BsonElement("contents")]
    public List<UnknownGenericResultDto>? Contents { get; set; }
    
    [BsonElement("sharedWith")]
    public List<UserBasicDetailsDto>? SharedWith { get; set; }
    
}