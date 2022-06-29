namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class GetUsersFollowingPlaylistDto
{
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}