using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Models.DTOs.Playlist;

public class PlaylistInformationWithContentsDto : PlaylistInformationDto
{

    [BsonElement("contents")]
    public List<UnknownGenericResultDto>? Contents { get; set; }
    
    [BsonElement("sharedWith")]
    public List<UserBasicDetailsDto>? SharedWith { get; set; }
    
}