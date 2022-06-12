using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.Database
{
    public class RecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("viewDates")] public List<DateTime> ViewDates { get; set; }
        [BsonElement("viewsAmount")] public int ViewsAmount { get; set; }
        
        [BsonElement("generalizedResult")] public UnknownGeneralizedResultDto GeneralizedResult { get; set; }

        public RecommendationsDocument(SaveViewDto request)
        {
            ViewDates = new List<DateTime> {DateTime.Now};
            ViewsAmount = 1;
            GeneralizedResult = request.GeneralizedResult;
        }
    }
}