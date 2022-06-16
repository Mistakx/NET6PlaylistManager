namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class ToggleUserFollowDto
{
    public string UserId { get; set; }
    public string SessionToken { get; set; }
}