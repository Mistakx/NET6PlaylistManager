namespace SkyPlaylistManager.Models.DTOs.UserRequests;

public class GetUserPlaylistsDto
{
    public string Username { get; set; }
    public string SessionToken { get; set; }
}