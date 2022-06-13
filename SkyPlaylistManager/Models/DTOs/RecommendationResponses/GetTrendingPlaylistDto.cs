using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingPlaylistDto
{
    [BsonElement("playlist")] public PlaylistBasicDetailsDto Playlist { get; set; }
}