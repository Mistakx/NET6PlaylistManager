namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class SortContentsDto
{
    public string PlaylistId { get; set; }
    public string GeneralizedResultDatabaseId { get; set; }
    public int NewIndex { get; set; }
    public string SessionToken { get; set; }
}