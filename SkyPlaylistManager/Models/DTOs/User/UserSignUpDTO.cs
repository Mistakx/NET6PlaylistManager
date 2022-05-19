namespace SkyPlaylistManager.Models.DTOs.User {

    public class NewUserDto {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Username { get; set; } = null!;
    }

}
