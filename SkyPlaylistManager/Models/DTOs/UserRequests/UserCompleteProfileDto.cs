using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;

namespace SkyPlaylistManager.Models.DTOs.UserRequests
{
    public class UserCompleteProfileDto : UserBasicProfileDto
    {
        [BsonElement("userPlaylists")] public List<PlaylistInformationDto>? UserPlaylists { get; set; }
    }
}