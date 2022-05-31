using System.Text.Json.Nodes;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.GeneralizedResults;

public class GeneralizedLivestreamResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }

    [BsonElement("gameName")] private string GameName { get; set; }

    public GeneralizedLivestreamResult(JsonObject request): base(request)
    {
        ResultType = "GenericLivestreamResult";
        this.GameName = (string?) request["gameName"]!;
    }

}