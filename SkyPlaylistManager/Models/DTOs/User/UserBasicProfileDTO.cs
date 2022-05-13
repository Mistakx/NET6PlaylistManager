using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace SkyPlaylistManager.Models.DTOs.User {
    
    public class UserBasicDetailsDto {
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }
        
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("username")] 
        public string? Username { get; set; }

        [BsonElement("profilePhotoUrl")]
        public string? ProfilePhotoUrl { get; set; }
        
    }


}
