using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class SaveUserViewDto
{
    public string UserId { get; set; }
    public string SessionToken { get; set; }
}