using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.HubRequests
{
    public class ConnectedUserDto
    {
        public string sessionToken { get; set; }
        public string hubconnectionId { get; set; }
    }
}
