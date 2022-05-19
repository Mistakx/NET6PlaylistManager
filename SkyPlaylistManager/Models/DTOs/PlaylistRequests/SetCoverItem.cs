namespace SkyPlaylistManager.Models.DTOs.PlaylistRequests
{
    public class SetCoverItem
    {
        public string? CoverUrl { get; set; }
        public string? PlaylistId { get; set; }
        public string? SessionToken { get; set; }
    }
}
