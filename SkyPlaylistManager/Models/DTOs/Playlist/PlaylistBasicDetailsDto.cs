using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.Playlist;

public class PlaylistBasicDetailsDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = null!;
        
    [BsonElement("visibility")]
    public string Visibility { get; set; } = null!;

    [BsonElement("creationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? CreationDate { get; set; } = null!;

    [BsonElement("description")] 
    public string? Description { get; set; }
}

