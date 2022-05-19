namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests
{
    public class CreatePlaylistDto 
    {
        public string Title { get; set; }
        public string Visibility { get; set; }
        public string? Description { get; set; }
        public string? SessionToken { get; set; }
    }
}
