namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetTrendingUsersDto
{
    public string Username { get; set; }
    public int Limit { get; set; }
    public int PageNumber {get; set;}
    public string SessionToken { get; set; }
}