namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class TogglePlaylistFollowDto
{
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}