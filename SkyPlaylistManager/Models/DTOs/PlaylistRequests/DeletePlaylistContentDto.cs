namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class DeletePlaylistContentDto
{
    public string? PlaylistId { get; set; }
    public string? MultimediaContentId { get; set; }
}