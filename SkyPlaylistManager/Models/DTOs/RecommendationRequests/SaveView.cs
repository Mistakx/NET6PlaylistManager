using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class SaveViewDto
{
    public UnknownGeneralizedResultDto GeneralizedResult { get; set; }
    public string SessionToken { get; set; }
}