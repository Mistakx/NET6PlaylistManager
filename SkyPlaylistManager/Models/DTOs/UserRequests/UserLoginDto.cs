using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.UserRequests {
    
    public class LoginDto {
    
        public string Email { get; set; }

        public string Password { get; set; }

    }

}
