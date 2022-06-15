using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingUsersLookupDto : UserRecommendationsDocument
{
    [BsonElement("user")] public UserDocument User { get; set; }

    public GetTrendingUsersLookupDto(string userId) : base(userId)
    {
    }
}