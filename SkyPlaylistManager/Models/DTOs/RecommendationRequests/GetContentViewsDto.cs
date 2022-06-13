using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetContentViewsDto
{
    public string PlayerFactoryName { get; set; }
    public string PlatformId { get; set; }
    public string? PlatformPlayerUrl { get; set; }
}