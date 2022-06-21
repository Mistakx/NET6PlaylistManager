namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class RemoveFollowFromPlaylistDto
{
    public string PlaylistId { get; set; }
    public string Username { get; set; }
    public string SessionToken { get; set; }
}