using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs
{
    public class UnknownGeneralizedResultDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? DatabaseId { get; set; }

        [BsonElement("interface")]
        public string? Interface { get; set; }
        
        [BsonElement("playerFactoryName")] 
        public string? PlayerFactoryName { get; set; }
        
        [BsonElement("platformPlayerUrl")] 
        public string? PlatformPlayerUrl { get; set; }
        
        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("platform")]
        public string? Platform { get; set; }

        [BsonElement("_t")]
        public string? Type { get; set; }

        [BsonElement("id")]
        public string? Id { get; set; }

        [BsonElement("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [BsonElement("creator")]
        public string? Creator { get; set; }

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; }
        
        [BsonElement("duration")]
        public double? Duration { get; set; }
        
        [BsonElement("views")]
        public int Views { get; set; }
        
        [BsonElement("category")]
        public string? Category { get; set; }
        
        [BsonElement("albumName")]
        public string AlbumName { get; set; }
        
        [BsonElement("gameName")]
        public string GameName { get; set; }
        
        [BsonElement("usages")]
        public int Usages { get; set; }
    }
}
