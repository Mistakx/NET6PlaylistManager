using System.Collections.Immutable;
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

        public async Task <List<UserDocument>> GetUserFriends(string databaseUserId) 
        {
            var myFollowers = await _communityService.GetUsersFollowingUser(databaseUserId);
            var myFollowedUsers = await _communityService.GetFollowedUsers(databaseUserId);

            var mutualFollowers = myFollowers.Where(followerUser => myFollowedUsers.Any(followedUser => followedUser.Id == followerUser.Id)).ToList();

            return mutualFollowers;
        }

        public async Task NotifyMyFriends(string databaseUserId, string message)
        {
            var userFriends = await GetUserFriends(databaseUserId);

            foreach (var friend in userFriends)
            {
                await Clients.Client(_connections[friend.Id]).SendAsync("notify", message);
                await GetAndSendUserOnlineFriends(friend.Id);
            }
        }



        public async Task UserConnected(ConnectedUserDto request)
        {
            var databaseUserId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
            var connectedUserInformation = await _usersService.GetUserById(databaseUserId);

            _connections[databaseUserId] = request.hubconnectionId;

            var message = connectedUserInformation.Username + " (" + connectedUserInformation.Name + ") is now Online.";
            await NotifyMyFriends(databaseUserId, message);

            await GetAndSendUserOnlineFriends(databaseUserId);
        }

   

        public async Task UserDisconnected(ConnectedUserDto request)
        {
            var databaseUserId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
            var connectedUserInformation = await _usersService.GetUserById(databaseUserId);

            var message = connectedUserInformation.Username + " (" + connectedUserInformation.Name + ") is now Offline.";
            await NotifyMyFriends(databaseUserId, message);

            try
            {
                _connections.Remove(databaseUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public async Task GetAndSendUserOnlineFriends(string databaseUserId)
        {
            var userFriends = await GetUserFriends(databaseUserId);

            var userHubConnectionId = _connections[databaseUserId];

            List<String> userKeys = _connections.Keys.ToList(); 

            var userOnlineFriends = userFriends.Where(user => userKeys.Contains(user.Id));

            await Clients.Client(userHubConnectionId).SendAsync("myOnlineFriends", userOnlineFriends);
        }

        public async Task GetOnlineFriends(string sessionToken)
        {
            var databaseUserId = _sessionTokensService.GetUserIdFromToken(sessionToken);
            await GetAndSendUserOnlineFriends(databaseUserId);
        }



    }

}