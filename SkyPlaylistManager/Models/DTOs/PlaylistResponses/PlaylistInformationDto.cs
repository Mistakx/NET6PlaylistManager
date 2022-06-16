namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public int ResultsAmount { get; set; }
    public string? Visibility { get; set; }
    public int? WeeklyViewsAmount { get; set; }
    public int? TotalViewsAmount { get; set; }
    public bool? Followed { get; set; }

    public PlaylistInformationDto(string playlistId, string title, string description, string thumbnailUrl, int resultsAmount)
    {
        Id = playlistId;
        Title = title;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        ResultsAmount = resultsAmount;
    }
    
}