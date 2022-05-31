namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class SortContentsDto
{
    public string PlaylistId { get; set; }
    public string GeneralizedResultId { get; set; }
    public int NewIndex { get; set; }
}