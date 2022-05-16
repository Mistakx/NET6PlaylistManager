using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Models.Database
{
    public class PlaylistCollection
    {
        private readonly SessionTokensService sessionTokensService;

        public PlaylistCollection(SessionTokensService sessionTokensService)
        {
            this.sessionTokensService = sessionTokensService;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")] public string? Title { get; set; }

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; }

        [BsonElement("visibility")] public string Visibility { get; set; }

        [BsonElement("owner")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Owner { get; set; }

        [BsonElement("sharedWith")] public List<ObjectId>? SharedWith { get; set; }

        [BsonElement("contents")] public List<ObjectId>? Contents { get; set; }

        [BsonElement("description")] public string Description { get; set; }


        public PlaylistCollection(NewPlaylistDto request)
        {
            Title = request.Title;
            CreationDate = DateTime.Now;
            Visibility = request.Visibility!;
            Description = request.Description!;
            Owner = sessionTokensService.GetUserId(request.SessionToken!);
            SharedWith = new List<ObjectId>();
            Contents = new List<ObjectId>();
        }
    }
}