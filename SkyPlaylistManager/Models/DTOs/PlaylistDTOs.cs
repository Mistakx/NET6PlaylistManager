using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Models.DTOs
{
    public class NewPlaylistDto
    {
        public string Title { get; set; } = null!;
        public string Visibility { get; set; } = null!;
        public string Description { get; set; } = null!;
    }


    public class PlaylistShareDto
    {
        public string PlaylistID { get; set; } = null!;
        public string UserID { get; set; } = null!;
    }


    public class PlaylistBasicDetailsDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;
        
        [BsonElement("visibility")]
        public string Visibility { get; set; } = null!;

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; } = null!;

        [BsonElement("description")] 
        public string? Description { get; set; }
    }



    public class PlaylistAndContentsDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("owner")]
        public UserBasicDetailsDto? Owner { get; set; } 
        
        [BsonElement("visibility")]
        public string? Visibility { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; }

        [BsonElement("contents")]
        public List<GenericMultimediaContentDto>? Contents { get; set; }

        [BsonElement("sharedWith")]
        public List<UserBasicDetailsDto>? SharedWith { get; set; }
    }
    
    public class EditTitleDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string NewTitle { get; set; } = null!;
    }

    public class EditDescriptionDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string NewDescription { get; set; } = null!;
    }

    public class EditVisibilityDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string NewVisibility { get; set; } = null!;
    }

    public class DeletePlaylistDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
    }

    public class PlaylistShareDTO
    {
       
        public string? PlaylistID { get; set; }
       
        public string? UserID { get; set; }
    }

    public class DeletePlaylistContentDTO
    {
        public string? PlaylistID { get; set; }
        public string? MultimediaContentID { get; set; }
    }
}
