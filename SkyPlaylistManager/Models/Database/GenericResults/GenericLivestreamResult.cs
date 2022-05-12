﻿using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database.GenericResults;

namespace SkyPlaylistManager.Models.Database
{

    public class GenericLivestreamResult : GenericResult
    {
        
        [BsonElement("interface")] public string? Interface { get; set; }

        [BsonElement("gameName")] public string? GameName { get; set; }

        public GenericLivestreamResult(JsonObject request): base(request)
        {
            this.Interface = "GenericLivestreamResult";
            this.GameName = (string?)request["gameName"];
        }
    }


}
