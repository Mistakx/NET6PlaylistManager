using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class GetPlaylistContentLookupDto : PlaylistDocument
{
    [BsonElement("content")] public List<UnknownContentDocumentDto> Content { get; set; }

    public GetPlaylistContentLookupDto(CreatePlaylistDto request, SessionTokensService sessionTokensService) : base(request, sessionTokensService)
    {
    }
}