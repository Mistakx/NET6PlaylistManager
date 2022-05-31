using System.Text.Json.Nodes;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.GeneralizedResults;

public class GeneralizedVideoResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }
    
    public GeneralizedVideoResult(JsonObject request): base(request)
    {
        ResultType = "GenericVideoResult";
    }

}