namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetUserViewsDto
{
    public string Username { get; set; }
    public string SessionToken { get; set; }
}