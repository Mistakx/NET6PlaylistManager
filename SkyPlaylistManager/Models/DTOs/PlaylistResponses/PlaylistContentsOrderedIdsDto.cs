using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistContentsOrderedIdsDto
{
    [BsonElement("resultIds")] public List<ObjectId> ResultIds { get; set; }
}