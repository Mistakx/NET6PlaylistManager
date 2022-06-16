namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class SetPlaylistCoverDto
{
    public string CoverUrl { get; set; }
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}