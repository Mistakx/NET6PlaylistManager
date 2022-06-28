using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationDto
{
    public string Id { get; }
    public string Title { get; }
    public string? Description { get; }
    public string ThumbnailUrl { get; }
    public int? ResultsAmount { get; }

    public string? Visibility { get; set; }
    
    public int? WeeklyViewsAmount { get; set; }
    public int? TotalViewsAmount { get; set; }
    
    public bool? Followed { get; set; }
    
    public int? FollowersAmount { get; set; }
    
    public UserProfileDto? Owner { get; set; }

    public PlaylistInformationDto(string playlistId, string title, string description, string thumbnailUrl,
        int resultsAmount)
    {
        Id = playlistId;
        Title = title;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        ResultsAmount = resultsAmount;
    }
    
    public PlaylistInformationDto(PlaylistDocument playlistDocument)
    {
        Id = playlistDocument.Id;
        Title = playlistDocument.Title;
        ThumbnailUrl = playlistDocument.ThumbnailUrl;
    }
}