using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.UserRequests;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationWithContentsDto : PlaylistInformationDto
{
    [BsonElement("contents")] public List<UnknownGeneralizedResultDto>? Contents { get; set; }

    [BsonElement("sharedWith")] public List<UserBasicProfileDto>? SharedWith { get; set; }
}