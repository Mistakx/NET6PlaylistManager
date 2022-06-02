using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.Database
{
    public class PlaylistDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")] public string Title { get; set; }

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDate { get; set; }

        [BsonElement("visibility")] public string Visibility { get; set; }

        [BsonElement("owner")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Owner { get; set; }

        [BsonElement("contents")] public List<ObjectId>? Contents { get; set; }
        [BsonElement("description")] public string Description { get; set; }
        [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }

        public PlaylistDocument(CreatePlaylistDto request, SessionTokensService sessionTokensService)
        {
            Title = request.Title;
            CreationDate = DateTime.Now;
            Description = request.Description;
            Visibility = request.Visibility;
            Owner = sessionTokensService.GetUserId(request.SessionToken!);
            Contents = new List<ObjectId>();
        }
    }
}