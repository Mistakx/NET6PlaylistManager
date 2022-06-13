using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.Database
{
    public class UserRecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("weeklyViewDates")]
        public List<DateTime> WeeklyViewDates { get; set; }

        [BsonElement("weeklyViewsAmount")] public int WeeklyViewsAmount { get; set; }
        [BsonElement("totalViewsAmount")] public int TotalViewsAmount { get; set; }

        [BsonElement("UserId")] public string UserId { get; set; }

        public UserRecommendationsDocument(SaveUserViewDto request)
        {
            WeeklyViewDates = new List<DateTime> {DateTime.Now};
            WeeklyViewsAmount = 1;
            TotalViewsAmount = 1;
            UserId = request.UserId;
        }
    }
}