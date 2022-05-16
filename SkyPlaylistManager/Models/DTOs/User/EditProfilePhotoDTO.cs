namespace SkyPlaylistManager.Models.DTOs.User
{
    public class EditProfilePhotoDto
    {
        public IFormFile? UserPhoto { get; set; }
        public string? SessionToken { get; set; }
    }
}