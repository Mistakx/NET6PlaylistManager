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
        [BsonElement("views")] public List<DateTime> Views { get; set; }
        
        [BsonElement("generalizedResult")] public UnknownGeneralizedResultDto GeneralizedResult { get; set; }

        public RecommendationsDocument(SaveViewDto request)
        {
            Views = new List<DateTime>();
            GeneralizedResult = request.GeneralizedResult;
        }
    }
}