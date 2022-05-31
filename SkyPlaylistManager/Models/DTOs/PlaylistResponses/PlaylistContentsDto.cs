using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistContentsDto
{
    [BsonElement("contents")] 
    public List<UnknownGeneralizedResultDto>? Contents { get; set; }
}