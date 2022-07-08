using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.ContentResponses;

public class UnknownContentResponseDto
{
    public string? DatabaseId { get; set; }
    public string? PlatformName { get; set; }
    public string ResultType { get; set; }
    public string PlayerFactoryName { get; set; }
    public string Title { get; set; }
    public string PlatformId { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Creator { get; set; }
    public string? PlatformPlayerUrl { get; set; }
    
    // Track
    public string? AlbumName { get; set; }
    
    // Podcast 
    public string? Url { get; set; }
    public string? Href { get; set; }
    
    // Livestream
    public string? GameName { get; set; }
    
    // Radio
    public string? Website { get; set; }
    public string? Region { get; set; }


    public int? WeeklyViewsAmount { get; set; }
    public int? TotalViewsAmount { get; set; }

    public UnknownContentResponseDto(UnknownContentDocumentDto request)
    {
        DatabaseId = request.DatabaseId;
        PlatformName = request.PlatformName;
        ResultType = request.ResultType;
        PlayerFactoryName = request.PlayerFactoryName;
        Title = request.Title;
        PlatformId = request.PlatformId;
        ThumbnailUrl = request.ThumbnailUrl;
        Creator = request.Creator;
        PlatformPlayerUrl = request.PlatformPlayerUrl;
        AlbumName = request.AlbumName;
        GameName = request.GameName;
        Region = request.Region;
        Url = request.Url;
        Href = request.Href;
        Website = request.Website;
    }
}