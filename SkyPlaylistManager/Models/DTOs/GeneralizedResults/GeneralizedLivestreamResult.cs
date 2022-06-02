using System.Text.Json.Nodes;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.GeneralizedResults;

public class GeneralizedLivestreamResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }

    [BsonElement("gameName")] private string GameName { get; set; }

    public GeneralizedLivestreamResult(UnknownGeneralizedResultDto request): base(request)
    {
        ResultType = "GenericLivestreamResult";
        GameName = request.GameName!;
    }

}