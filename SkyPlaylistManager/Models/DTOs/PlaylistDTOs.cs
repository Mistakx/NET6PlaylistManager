﻿using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs
{
    public class NewPlaylistDto
    {
        public string Title { get; set; } = null!;
        public string Visibility { get; set; } = null!;

    }





    public class PlaylistContentsDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("owner")]
        public ObjectId Owner { get; set; } 
        
        [BsonElement("visibility")]
        public string Visibility { get; set; } = null!;
        
        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; } = null!;

        [BsonElement("contents")]
        public List<GenericMultimediaContentDto>? Contents { get; set; }

        [BsonElement("sharedWith")]
        public List<ObjectId>? SharedWith { get; set; }
    }
}
