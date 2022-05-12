using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database.GenericResults;

namespace SkyPlaylistManager.Models.Database
{
    
    public class GenericVideoResult : GenericResult
    {
        
        [BsonElement("interface")] public string Interface { get; set; }

        public GenericVideoResult(JsonObject request): base(request)
        {
            this.Interface = "GenericVideoResult";
        }
    }

}
