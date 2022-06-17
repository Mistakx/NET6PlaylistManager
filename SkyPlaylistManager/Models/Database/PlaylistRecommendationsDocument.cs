using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;

namespace SkyPlaylistManager.Models.Database
{
    public class PlaylistRecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("weeklyViewDates")] public List<DateTime> WeeklyViewDates { get; set; }
        [BsonElement("totalViewsAmount")] public int TotalViewsAmount { get; set; }
        
        [BsonElement("playlistId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlaylistId { get; set; }
        public PlaylistRecommendationsDocument(SavePlaylistViewDto request)
        {
            WeeklyViewDates = new List<DateTime> {DateTime.Now};
            TotalViewsAmount = 1;
            PlaylistId = request.PlaylistId;
        }
    }
}