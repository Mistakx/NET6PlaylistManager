using MongoDB.Bson;

namespace SkyPlaylistManager.Models.DTOs.Playlist
{
    public class SortContentsDTO
    {
        public string PlaylistId { get; set; }

        public string MultimediaContentId { get; set; }

        public int NewPosition { get; set; }
    }
}
