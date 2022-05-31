namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class DeletePlaylistContentDto
{
    public string PlaylistId { get; set; }
    public string GeneralizedResultDatabaseId { get; set; }
    public string SessionToken { get; set; }
}