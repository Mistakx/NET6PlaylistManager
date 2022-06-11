using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserPlaylistsDto
{
    [BsonElement("playlists")] public List<PlaylistDocument> Playlists { get; set; }
}