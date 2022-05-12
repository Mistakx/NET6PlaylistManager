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

        [BsonElement("id")] private string? PlatformId { get; set; }

        [BsonElement("title")] private string? Title { get; set; }

        [BsonElement("thumbnailUrl")] private string? ThumbnailUrl { get; set; }

        [BsonElement("creator")] private string? Creator { get; set; }

        [BsonElement("playerFactoryName")] private string? PlayerFactoryName { get; set; }
        
        [BsonElement("platformPlayerUrl")] private string? PlatformPlayerUrl { get; set; }

        protected GenericResult(JsonObject request)
        {
            this.PlatformId = (string?) request["id"];
            this.Title = (string?) request["title"];
            this.ThumbnailUrl = (string?) request["thumbnailUrl"];
            this.Creator = (string?) request["creator"];
            this.PlayerFactoryName = (string?) request["playerFactoryName"];
            this.PlatformPlayerUrl = (string?) request["platformPlayerUrl"];
        }
        
    }

}
 