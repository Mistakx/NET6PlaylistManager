using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class SaveContentViewDto
{
    public UnknownContentDocumentDto Content { get; set; }
    public string SessionToken { get; set; }
}