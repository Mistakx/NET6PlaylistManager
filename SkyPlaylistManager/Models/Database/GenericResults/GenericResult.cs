using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.Database.GenericResults
{
    
    public abstract class GenericResult
    {
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("id")] public string? PlaftormId { get; set; }

        [BsonElement("titCle")] public string? Title { get; set; }

        [BsonElement("thumbnailUrl")] public string? ThumbnailUrl { get; set; }

        [BsonElement("creator")] public string? Creator { get; set; }

        // [BsonElement("playerFactory")] public Object? PlayerFactory { get; set; }

        protected GenericResult(JsonObject request)
        {
            this.PlaftormId = (string?) request["id"];
            this.Title = (string?) request["title"];
            this.ThumbnailUrl = (string?) request["thumbnailUrl"];
            this.Creator = (string?) request["creator"];
            // this.PlayerFactory = request["playerFactory"];
        }
        
    }

}
 