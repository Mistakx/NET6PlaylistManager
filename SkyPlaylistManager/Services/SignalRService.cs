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
            GetMyOnlineFriends(databaseUserId);
        }


        public async Task UserDisconnected(ConnectedUserDto request)
        {
            var databaseUserId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
            var connectedUserInformation = await _usersService.GetUserById(databaseUserId);

            var message = connectedUserInformation.Username + " (" + connectedUserInformation.Name + ") is now Offline.";
            NotifyMyFriends(databaseUserId, message);

            _connections.Remove(databaseUserId);
        }


        public async Task GetMyOnlineFriends(string databaseUserId)
        {
            var myFollowers = await _communityService.GetUsersFollowingUser(databaseUserId);

            var myHubConnectionId = _connections[databaseUserId];

            List<String> userKeys = _connections.Keys.ToList(); 

            var myOnlineFollowers = myFollowers.Where(user => userKeys.Contains(user.Id));

            await Clients.Client(myHubConnectionId).SendAsync("myFriends", myOnlineFollowers);

        }


    }

}