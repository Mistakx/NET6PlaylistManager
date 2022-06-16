namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetTrendingContentDto
{
    public int Limit { get; set; }
    public int PageNumber {get; set;}
    public string SessionToken { get; set; }
}