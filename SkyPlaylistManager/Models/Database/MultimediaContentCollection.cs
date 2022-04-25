using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Models.Database
{
    public abstract class MultimediaContent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public abstract string? Id { get; set; }

        [BsonElement("title")]
        public abstract string Title { get; set; }

        [BsonElement("platform")]
        public abstract string Platform { get; set; } 

        [BsonElement("platformId")]
        public abstract string PlatformId { get; set; } 

        [BsonElement("thumbnailUrl")]
        public abstract string ThumbnailUrl { get; set; }

        [BsonElement("creator")]
        public abstract string Creator { get; set; }

        [BsonElement("creationDate")]
        public abstract string CreationDate { get; set; }
        //DATA
        //NUMBER OF USAGES


    }

    public class VideosContent : MultimediaContent
    {
        public override string? Id { get; set; }

        public override string Title { get; set; } = null!;

        public override string Platform { get; set; } = null!;

        public override string PlatformId { get; set; } = null!;

        public override string ThumbnailUrl { get; set; } = null!;

        public override string Creator { get; set; } = null!;

        public override string CreationDate { get; set; } = null!;

        [BsonElement("duration")]
        public double Duration { get; set; }

        [BsonElement("views")]
        public int Views { get; set; }
    }

    public class TracksContent : MultimediaContent
    {
        public override string? Id { get; set; }

        public override string Title { get; set; } = null!;

        public override string Platform { get; set; } = null!;

        public override string PlatformId { get; set; } = null!;

        public override string ThumbnailUrl { get; set; } = null!;

        public override string Creator { get; set; } = null!;

        public override string CreationDate { get; set; } = null!;

        [BsonElement("duration")]
        public double Duration { get; set; }
    }

    public class LivestreamsContent : MultimediaContent
    {
        public override string? Id { get; set; }

        public override string Title { get; set; } = null!;

        public override string Platform { get; set; } = null!;

        public override string PlatformId { get; set; } = null!;

        public override string ThumbnailUrl { get; set; } = null!;

        public override string Creator { get; set; } = null!;

        public override string CreationDate { get; set; } = null!;

        [BsonElement("category")]
        public string Category { get; set; }
    }


}
