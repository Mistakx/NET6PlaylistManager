using Microsoft.AspNetCore.SignalR;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.HubRequests;

namespace SkyPlaylistManager.Services
{




    public class SignalRService : Hub
    {
        private readonly UsersService _usersService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly CommunityService _communityService;
        private Dictionary<string, string> _connections;

        public SignalRService(UsersService usersService, SessionTokensService sessionTokensService, CommunityService communityService)
        {
            _usersService = usersService;
            _sessionTokensService = sessionTokensService;
            _communityService = communityService;
            _connections = new Dictionary<string, string>();
        }

        public async Task NotifyMyFriends(string databaseUserId, string message)
        {
            var myFollowers = await _communityService.GetUsersFollowingUser(databaseUserId);
           

            foreach (var follower in myFollowers)
            {
                await Clients.Client(_connections[follower.Id]).SendAsync("notify", message);
            }

        }

        public async Task UserConnected(ConnectedUserDto request)
        {
            var databaseUserId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
            var connectedUserInformation = await _usersService.GetUserById(databaseUserId);

            _connections[databaseUserId] = request.hubconnectionId;

            var message = connectedUserInformation.Username + " (" + connectedUserInformation.Name + ") is now Online.";
            NotifyMyFriends(databaseUserId, message);
            
        }

      


        public async Task NewMessage(string message) =>
            await Clients.All.SendAsync("messageReceived", message);

    }

}