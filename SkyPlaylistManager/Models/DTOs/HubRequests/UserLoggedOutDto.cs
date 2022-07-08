namespace SkyPlaylistManager.Models.DTOs.HubRequests
{
    public class UserLoggedOutDto
    {
        public string SessionToken { get; set; }
        public string HubConnectionId { get; set; }
    }
}
