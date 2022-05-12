using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database.GenericResults;

namespace SkyPlaylistManager.Models.Database.GenericResults
{

    public class GenericLivestreamResult : GenericResult
    {
        
        [BsonElement("interface")] private string? Interface { get; set; }

        [BsonElement("gameName")] private string? GameName { get; set; }

        public GenericLivestreamResult(JsonObject request): base(request)
        {
            this.Interface = "GenericLivestreamResult";
            this.GameName = (string?) request["gameName"];
        }
    }


}
