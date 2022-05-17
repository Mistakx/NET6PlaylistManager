using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests
{
    public class PlaylistContentsDto
    {
        [BsonElement("contents")] 
        public List<UnknownGeneralizedResultDto>? Contents { get; set; }
    }
}
