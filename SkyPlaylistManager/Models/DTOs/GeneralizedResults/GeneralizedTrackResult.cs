using System.Text.Json.Nodes;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.GeneralizedResults;

public class GeneralizedTrackResult : GeneralizedResult
{
        
    public sealed override string ResultType { get; set; }

    [BsonElement("albumName")] private string AlbumName { get; set; }

    public GeneralizedTrackResult(UnknownGeneralizedResultDto request): base(request)
    {
        ResultType = "GenericTrackResult";
        AlbumName = request.AlbumName!;
    }

}