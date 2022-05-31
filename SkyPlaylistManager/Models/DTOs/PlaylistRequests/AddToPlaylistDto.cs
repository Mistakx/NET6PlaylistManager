using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests;

public class AddToPlaylistDto: GeneralizedResult
{
    public string PlaylistId { get; set; }

    public AddToPlaylistDto(JsonObject request) : base(request)
    {
    }

    public override string ResultType { get; set; }
}