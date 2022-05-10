using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace SkyPlaylistManager.Models.DTOs.User {
    
    public class UserBasicDetailsDto {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = null!;
        
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("profilePhotoPath")]
        public string ProfilePhotoPath { get; set; } = null!;
    }


}
