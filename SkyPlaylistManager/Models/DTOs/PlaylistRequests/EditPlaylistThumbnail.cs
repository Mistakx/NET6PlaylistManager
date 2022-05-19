namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests
{
    public class EditPlaylistThumbnail
    {
        public IFormFile? PlaylistPhoto { get; set; }
        public string? PlaylistId { get; set; }
        public string? SessionToken { get; set; }
    }
}