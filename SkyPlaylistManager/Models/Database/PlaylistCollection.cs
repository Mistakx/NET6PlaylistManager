using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models.Database
{


    public class PlaylistCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; }

        [BsonElement("visibility")]
        public string Visibility { get; set; } = null!;

        [BsonElement("owner")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Owner { get; set; } = null!;

        [BsonElement("sharedWith")] 
        public List<ObjectId>? SharedWith { get; set; }

        [BsonElement("contents")]
        public List<ObjectId>? Contents { get; set; }
        [BsonElement("description")]
        public string Description { get; set; } = null!;


        public PlaylistCollection(NewPlaylistDto newPlaylist)
        {
            Title = newPlaylist.Title;
            CreationDate = DateTime.Now;
            Visibility = newPlaylist.Visibility;
            Description = newPlaylist.Description;
            Owner = "6261707eff67ad3d4f51d38b";  // TODO: Change to session user ID
            SharedWith = new List<ObjectId>();
            Contents = new List<ObjectId>();
        }

       
    }
}
