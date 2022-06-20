namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class SortUserFollowDto
{
    public string Username { get; set; }
    public int NewIndex { get; set; }
    public string SessionToken { get; set; }
}