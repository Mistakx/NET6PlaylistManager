using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingContentLookupDto
{
    [BsonElement("generalizedResult")] public UnknownContentDocumentDto generalizedResult { get; set; }
}