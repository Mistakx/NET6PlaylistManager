namespace SkyPlaylistManager.Models.DTOs.UserRequests
{
    public class EditProfilePhotoDto
    {
        public IFormFile? UserPhoto { get; set; }
        public string? SessionToken { get; set; }
    }
}