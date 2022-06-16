using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.UserRequests;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class GetUserPlaylistsLookupDto : UserDocument
{
    [BsonElement("playlists")] public List<PlaylistDocument> Playlists { get; set; }

    public GetUserPlaylistsLookupDto(UserSignupDto userSignup, string profilePhotoUrl) : base(userSignup, profilePhotoUrl)
    {
    }
}