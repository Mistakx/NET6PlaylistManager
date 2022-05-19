using System.Text.Json.Nodes;
using System.Transactions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database.GenericResults;

namespace SkyPlaylistManager.Models.Database.GenericResults
{
 
    public class GenericTrackResult : GenericResult
    {
        
        [BsonElement("interface")] private string? Interface { get; set; }
        [BsonElement("albumName")] private string? AlbumName { get; set; }

        public GenericTrackResult(JsonObject request): base(request)
        {
            this.Interface = "GenericTrackResult";
            this.AlbumName = (string?) request["albumName"];
        }
        
    }
    
}
