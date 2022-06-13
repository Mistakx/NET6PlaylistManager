using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class SaveContentViewDto
{
    public UnknownGeneralizedResultDto GeneralizedResult { get; set; }
    public string SessionToken { get; set; }
}