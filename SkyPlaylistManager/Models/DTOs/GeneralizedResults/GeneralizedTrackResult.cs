using System.Text.Json.Nodes;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.GeneralizedResults;

public class GeneralizedTrackResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }

    [BsonElement("albumName")] private string AlbumName { get; set; }

    public GeneralizedTrackResult(JsonObject request): base(request)
    {
        ResultType = "GenericTrackResult";
        AlbumName = (string) request["albumName"]!;
    }

}