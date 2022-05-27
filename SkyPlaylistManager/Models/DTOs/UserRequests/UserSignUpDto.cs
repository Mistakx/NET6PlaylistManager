namespace SkyPlaylistManager.Models.DTOs.UserRequests {

    public class UserSignupDto {
        
        public IFormFile? UserPhoto { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public string Name { get; set; }
        public string Username { get; set; }
    }

}
