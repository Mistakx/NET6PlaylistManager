using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class AddContentToPlaylistDto: UnknownGeneralizedResultDto
{
    public string PlaylistId { get; set; }
}