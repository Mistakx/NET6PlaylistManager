namespace SkyPlaylistManager.Models.DTOs.CommunityRequests;

public class SortUserFollowDto
{
    public string followedUserUsername { get; set; }
    public int NewIndex { get; set; }
    public string SessionToken { get; set; }
}