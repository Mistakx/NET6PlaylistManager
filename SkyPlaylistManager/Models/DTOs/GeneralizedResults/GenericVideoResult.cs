using System.Text.Json.Nodes;

namespace SkyPlaylistManager.Models.DTOs.GeneralizedResults;

public class GeneralizedVideoResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }
    
    public GeneralizedVideoResult(UnknownGeneralizedResultDto request): base(request)
    {
        ResultType = "GenericVideoResult";
    }

}