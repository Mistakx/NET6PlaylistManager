namespace SkyPlaylistManager.Models.DTOs.UserRequests;

public class EditPasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string SessionToken { get; set; }
}