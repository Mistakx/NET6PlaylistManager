namespace SkyPlaylistManager.Models.DTOs.UserRequests;

public class GetUserProfileDto
{
    public string Username { get; set; }
    public string SessionToken { get; set; }
}