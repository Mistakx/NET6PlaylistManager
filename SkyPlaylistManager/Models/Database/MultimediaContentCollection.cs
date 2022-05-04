using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public abstract DateTime? CreationDate { get; set; }
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

        public override DateTime? CreationDate { get; set; } = null!;

        [BsonElement("duration")]
        public double Duration { get; set; }

        [BsonElement("views")]
        public int Views { get; set; }

        public VideosContent(JsonObject request)
        {
            this.Title = (string)request["title"];
            this.Platform = (string)request["platform"];
            this.PlatformId = (string)request["platformId"];
            this.ThumbnailUrl = (string)request["thumbnailUrl"];
            this.Creator = (string)request["Creator"];
            this.CreationDate = DateTime.Now;
            this.Duration = (double)request["duration"];
            this.Views = (int)request["views"];
        }
    }

    public class TracksContent : MultimediaContent
    {
        public override string? Id { get; set; }

        public override string Title { get; set; } = null!;

        public override string Platform { get; set; } = null!;

        public override string PlatformId { get; set; } = null!;

        public override string ThumbnailUrl { get; set; } = null!;

        public override string Creator { get; set; } = null!;

        public override DateTime? CreationDate { get; set; } = null!;

        [BsonElement("duration")]
        public double Duration { get; set; } 

        public TracksContent(JsonObject request)
        {
            this.Title = (string)request["title"];
            this.Platform = (string)request["platform"];
            this.PlatformId = (string)request["platformId"];
            this.ThumbnailUrl = (string)request["thumbnailUrl"];
            this.Creator = (string)request["Creator"];
            this.CreationDate = DateTime.Now;
            this.Duration = (double)request["duration"];
        }
    }

    public class LivestreamsContent : MultimediaContent
    {
        public override string? Id { get; set; }

        public override string Title { get; set; } = null!;

        public override string Platform { get; set; } = null!;

        public override string PlatformId { get; set; } = null!;

        public override string ThumbnailUrl { get; set; } = null!;

        public override string Creator { get; set; } = null!;

        public override DateTime? CreationDate { get; set; } = null!;

        [BsonElement("category")]
        public string Category { get; set; } = null!;

        public LivestreamsContent(JsonObject request)
        {
            this.Title = (string)request["title"];
            this.Platform = (string)request["platform"];
            this.PlatformId = (string)request["platformId"];
            this.ThumbnailUrl = (string)request["thumbnailUrl"];
            this.Creator = (string)request["Creator"];
            this.CreationDate = DateTime.Now;
            this.Category = (string)request["category"];
        }
    }


}
