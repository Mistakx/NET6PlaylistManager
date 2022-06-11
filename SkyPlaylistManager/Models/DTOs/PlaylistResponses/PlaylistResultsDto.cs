using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistResultsDto
{
    [BsonElement("results")] public List<UnknownGeneralizedResultDto> Results { get; set; }
}