using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.ContentResponses;

public class UnknownContentResponseDto
{
    public string? DatabaseId { get; set; }
    public string ResultType { get; set; }
    public string PlayerFactoryName { get; set; }
    public string Title { get; set; }
    public string PlatformId { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Creator { get; set; }
    public string? PlatformPlayerUrl { get; set; }
    public string? AlbumName { get; set; }
    public string? GameName { get; set; }
    public int? WeeklyViewsAmount { get; set; }
    public int? TotalViewsAmount { get; set; }

    public UnknownContentResponseDto(UnknownContentDocumentDto request)
    {
        DatabaseId = request.DatabaseId;
        ResultType = request.ResultType;
        PlayerFactoryName = request.PlayerFactoryName;
        Title = request.Title;
        PlatformId = request.PlatformId;
        ThumbnailUrl = request.ThumbnailUrl;
        Creator = request.Creator;
        PlatformPlayerUrl = request.PlatformPlayerUrl;
        AlbumName = request.AlbumName;
        GameName = request.GameName;
    }
}