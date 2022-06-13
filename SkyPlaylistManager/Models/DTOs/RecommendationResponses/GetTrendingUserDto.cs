using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingUserDto
{
    [BsonElement("user")] public UserBasicProfileDto User { get; set; }
}