using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.Database
{
    public class PlaylistRecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("weeklyViewDates")] public List<DateTime> WeeklyViewDates { get; set; }
        [BsonElement("weeklyViewsAmount")] public int WeeklyViewsAmount { get; set; }
        [BsonElement("totalViewsAmount")] public int TotalViewsAmount { get; set; }
        [BsonElement("playlistName")] public string PlaylistName { get; set; }
        [BsonElement("playlistId")] public string PlaylistId { get; set; }
        public PlaylistRecommendationsDocument(SavePlaylistViewDto request)
        {
            WeeklyViewDates = new List<DateTime> {DateTime.Now};
            WeeklyViewsAmount = 1;
            TotalViewsAmount = 1;
            PlaylistId = request.PlaylistId;
            PlaylistName = request.PlaylistName;
        }
    }
}