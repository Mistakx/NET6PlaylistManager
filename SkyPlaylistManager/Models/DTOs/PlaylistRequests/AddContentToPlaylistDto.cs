using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class AddContentToPlaylistDto
{
    public string PlaylistId { get; set; }
    public UnknownContentDocumentDto Content { get; set; }

}