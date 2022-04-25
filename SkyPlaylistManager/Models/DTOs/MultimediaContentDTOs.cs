namespace SkyPlaylistManager.Models.DTOs
{
    public abstract class NewMultimediaContentDTO
    {
        public string Title { get; set; } = null!;
        public string Platform { get; set; } = null!;
        public string PlatformId { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;

    }

    public class NewVideoContentDto : NewMultimediaContentDTO
    {
        public double Duration { get; set; }
        public int Views { get; set; }
    }

    public class NewTrackContentDto : NewMultimediaContentDTO
    {
        public double Duration { get; set; }
    }

    public class NewLivestreamContentDto : NewMultimediaContentDTO
    {
        public string Category { get; set; }
    }

}
