namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class SortPlaylistsDto
{
    public string PlaylistId { get; set; }
    public int NewIndex { get; set; }
    public string SessionToken { get; set; }
}