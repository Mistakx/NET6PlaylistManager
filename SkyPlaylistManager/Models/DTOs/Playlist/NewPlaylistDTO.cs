namespace SkyPlaylistManager.Models.DTOs.Playlist;

public class NewPlaylistDto
{
    public string Title { get; set; } = null!;
    public string Visibility { get; set; } = null!;
    
    public string Description { get; set; } = null!;

}