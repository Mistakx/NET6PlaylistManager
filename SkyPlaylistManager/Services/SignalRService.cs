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
        private readonly Dictionary<string, List<string>> _userConnections;

        public SignalRService(UsersService usersService, SessionTokensService sessionTokensService,
            CommunityService communityService)
        {
            _usersService = usersService;
            _sessionTokensService = sessionTokensService;
            _communityService = communityService;
            _userConnections = new Dictionary<string, List<string>>();
        }

        // Receive Endpoints
        public async Task UserHasConnected(ConnectedUserDto request)
        {
            try
            {

                var userId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
                var connectedUserInformation = await _usersService.GetUserById(userId);

                Console.WriteLine("\nUser started connecting: " + connectedUserInformation?.Username);

                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Add(Context.ConnectionId);
                    Console.WriteLine("User was already connected. Not sending notifications to friends.");
                }
                else
                {
                    var message =
                        $"{connectedUserInformation?.Username} ({connectedUserInformation?.Name}) is now online.";

                    _userConnections.Add(userId, new List<string> { Context.ConnectionId });
                    Console.WriteLine("User wasn't connected yet. Sending notifications to friends.");
                    await SendUserConnectedNotification(userId, message);
                    await UpdateUserOnlineFriends(userId);
                }
                await UpdateUser(userId);

                Console.WriteLine("User Connected number of connections: " + _userConnections[userId].Count);
                Console.WriteLine("\nUser finished connecting.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task UserHasDisconnected(ConnectedUserDto request)
        {
            try
            {
                var userId = _sessionTokensService.GetUserIdFromToken(request.sessionToken);
                var connectedUserInformation = await _usersService.GetUserById(userId);
                
                _userConnections.Remove(userId);

                var message =
                    $"{connectedUserInformation?.Username} ({connectedUserInformation?.Name}) is now offline.";

                await SendUserConnectedNotification(userId, message);
                await UpdateUserOnlineFriends(userId);

                Console.WriteLine("User Disconnected: " + userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex){
            // var name = Context.User.Identity.Name;
            // var user = Context.User;
            // Console.WriteLine("Disconnecting to SignalRHub for User: {0}", name);
            //
            // var msg = "some message";
            // await Clients.Groups(name).SendAsync("jsListener", msg);
            //                
            // await base.OnDisconnectedAsync(ex);           
        }
        // Send Endpoints
        private async Task SendUserConnectedNotification(string userId, string message)
        {
            try
            {
                var userFriends = await _communityService.GetUserFriends(userId);

                var userKeys = _userConnections.Keys.ToList();

                var userOnlineFriends = userFriends.Where(user => userKeys.Contains(user.Id));

                foreach (var friend in userOnlineFriends)
                {
                    foreach (var connectionId in _userConnections[friend.Id])
                    {
                        await Clients.Client(connectionId).SendAsync("notify", message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public async Task UpdateUserOnlineFriends(string userId)
        {
            try
            {
                var userOnlineFriends = await GetUserOnlineFriends(userId);
                Console.WriteLine("Updating User Online Friends: " + userId);
                foreach (var friend in userOnlineFriends)
                {
                    Console.WriteLine("Friend: " + friend.Id);

                    var friendOnlineFriends = await GetUserOnlineFriends(friend.Id);


                    foreach (var connectionId in _userConnections[friend.Id])
                    {
                        await Clients.Client(connectionId).SendAsync("myOnlineFriends", friendOnlineFriends);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task UpdateUser(string userId)
        {
            try
            {
                var userOnlineFriends = await GetUserOnlineFriends(userId);
                Console.WriteLine("Updating User: " + userId);

                foreach (var connectionId in _userConnections[userId])
                {
                    await Clients.Client(connectionId).SendAsync("myOnlineFriends", userOnlineFriends);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task<IEnumerable<UserDocument>> GetUserOnlineFriends(string userId)
        {
            try
            {
                var userFriends = await _communityService.GetUserFriends(userId);
                var userOnlineFriends = userFriends.Where(user => _userConnections.ContainsKey(user.Id));
                return userOnlineFriends;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<UserDocument>();
            }
        }
    }
}