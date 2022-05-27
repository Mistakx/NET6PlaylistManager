using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Models.DTOs
{

    public class PlaylistBasicDetailsDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string? Title { get; set; }
        
        [BsonElement("visibility")]
        public string? Visibility { get; set; } 

        [BsonElement("creationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreationDate { get; set; }

        [BsonElement("description")] 
        public string? Description { get; set; }
    }



    public class PlaylistAndContentsDto
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
        public List<UnknownGenericResultDto>? Contents { get; set; }

        [BsonElement("sharedWith")]
        public List<UserBasicDetailsDto>? SharedWith { get; set; }
    }
    
    public class EditTitleDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? NewTitle { get; set; }
    }

    public class EditDescriptionDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? NewDescription { get; set; } 
    }

    public class EditVisibilityDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? NewVisibility { get; set; } 
    }

    public class DeletePlaylistDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
    }

    public class PlaylistShareDto
    {
       
        public string? PlaylistId { get; set; }
       
        public string? UserId { get; set; }
    }

    public class DeletePlaylistContentDto
    {
        public string? PlaylistId { get; set; }
        public string? MultimediaContentId { get; set; }
    }
}
