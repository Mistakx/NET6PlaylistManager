using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingPlaylistsLookupDto : PlaylistRecommendationsDocument
{
    [BsonElement("playlist")] public PlaylistDocument Playlist { get; set; }


    public GetTrendingPlaylistsLookupDto(SavePlaylistViewDto request) : base(request)
    {
    }
}