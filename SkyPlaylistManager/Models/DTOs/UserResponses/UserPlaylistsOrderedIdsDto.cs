using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserPlaylistContentsOrderedIdsDto
{
    [BsonElement("playlistIds")] public List<ObjectId> PlaylistIds { get; set; }
}