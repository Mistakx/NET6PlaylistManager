using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class AddGeneralizedResultPlaylistDto: UnknownGeneralizedResultDto
{
    public string PlaylistId { get; set; }
}