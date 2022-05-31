namespace SkyPlaylistManager.Models.DTOs.UserRequests;

public class EditUserInfoDto
{
    public string SessionToken { get; set; }
    public string NewEmail { get; set; }
    public string NewName { get; set; }
    public string NewUsername { get; set; }
}