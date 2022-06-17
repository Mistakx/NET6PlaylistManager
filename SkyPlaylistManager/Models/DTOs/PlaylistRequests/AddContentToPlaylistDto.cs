using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class AddContentToPlaylistDto: UnknownContentDocumentDto
{
    public string PlaylistId { get; set; }
}