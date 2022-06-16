namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class GetPlaylistInformationDto
{
    public string PlaylistId { get; set; }
    public string SessionToken { get; set; }
}