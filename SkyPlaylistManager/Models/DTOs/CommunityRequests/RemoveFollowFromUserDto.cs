namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class RemoveFollowFromUserDto
{
    public string Username { get; set; }
    public string SessionToken { get; set; }
}