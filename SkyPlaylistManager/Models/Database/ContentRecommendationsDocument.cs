using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;

namespace SkyPlaylistManager.Models.Database
{
    public class ContentRecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("weeklyViewDates")] public List<DateTime> WeeklyViewDates { get; set; }
        [BsonElement("totalViewsAmount")] public int TotalViewsAmount { get; set; }
        
        [BsonElement("generalizedResult")] public UnknownContentDocumentDto GeneralizedResult { get; set; }

        public ContentRecommendationsDocument(SaveContentViewDto request)
        {
            WeeklyViewDates = new List<DateTime> {DateTime.Now};
            TotalViewsAmount = 1;
            GeneralizedResult = request.Content;
        }
    }
}