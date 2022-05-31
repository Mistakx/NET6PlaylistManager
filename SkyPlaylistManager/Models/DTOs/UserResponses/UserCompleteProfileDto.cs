using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;

namespace SkyPlaylistManager.Models.DTOs.UserRequests;

public class UserCompleteProfileDto : UserBasicProfileDto
{
    [BsonElement("userPlaylists")] public List<PlaylistInformationDto>? UserPlaylists { get; set; }
}