namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class SortPlaylistFollowDto
{
    public string FollowedPlaylistId { get; set; }
    public int NewIndex { get; set; }
    public string SessionToken { get; set; }
}