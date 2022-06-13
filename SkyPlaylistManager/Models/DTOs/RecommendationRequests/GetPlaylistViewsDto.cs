namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetPlaylistViewsDto
{
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}