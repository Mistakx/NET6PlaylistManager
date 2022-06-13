namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class SavePlaylistViewDto
{
    public string PlaylistName { get; set; }
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}