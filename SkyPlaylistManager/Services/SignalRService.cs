using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.LiveRoomResponses;

namespace SkyPlaylistManager.Services
{
    public class SignalRService : Hub
    {
        private readonly UsersService _usersService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly DatabaseMigrationsService _databaseMigrationsService;
        private readonly CommunityService _communityService;
        private readonly List<UserConnection> _userConnections;

        public SignalRService(UsersService usersService, 
            SessionTokensService sessionTokensService,
            CommunityService communityService,
            DatabaseMigrationsService databaseMigrationsService)
        {
            _usersService = usersService;
            _sessionTokensService = sessionTokensService;
            _communityService = communityService;
            _databaseMigrationsService = databaseMigrationsService;
            _userConnections = new List<UserConnection>();
        }

        // Receive Endpoints
        // public async Task UserHasLoggedOut(UserLoggedOutDto request)
        // {
        //     try
        //     {
        //         var userId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
        //         var connectedUserInformation = await _usersService.GetUserById(userId);
        //
        //         UserConnection.RemoveUserId(_userConnections, userId);
        //
        //         var message =
        //             $"{connectedUserInformation?.Username} ({connectedUserInformation?.Name}) is now offline.";
        //
        //         await SendUserConnectedNotification(userId, message);
        //         await UpdateUserOnlineFriends(userId);
        //
        //         Console.WriteLine("User logged out: " + userId);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex);
        //     }
        // }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Console.WriteLine("starting migration");
                // await _databaseMigrationsService.UpdateDatabase();
                // Console.WriteLine("ended migration");
                
                var connectionId = Context.ConnectionId;
                var sessionToken = Context.GetHttpContext()?.Request.Query["sessionToken"];
                var userId = _sessionTokensService.GetUserIdFromToken(sessionToken!);

                var connectedUserInformation = await _usersService.GetUserById(userId);

                Console.WriteLine("\nUser started connecting: " + connectedUserInformation?.Username);

                if (UserConnection.ContainsUserId(_userConnections, userId))
                {
                    UserConnection.GetUserConnectionByUserId(_userConnections, userId).ConnectionIds.Add(connectionId);
                    Console.WriteLine("User was already connected. Not sending notifications to friends.");
                }
                else
                {
                    var message =
                        $"{connectedUserInformation?.Username} ({connectedUserInformation?.Name}) is now online.";

                    var newUser = new UserConnection
                    {
                        UserId = userId,
                    };
                    newUser.ConnectionIds.Add(connectionId);
                    _userConnections.Add(newUser);
                    Console.WriteLine("User wasn't connected yet. Sending notifications to friends.");
                    await SendUserConnectedNotification(userId, message);
                    await UpdateUserOnlineFriends(userId);
                }

                await UpdateUser(userId);

                Console.WriteLine("User Connected number of connections: " + UserConnection
                    .GetUserConnectionByUserId(_userConnections, userId).ConnectionIds.Count);

                Console.WriteLine("User finished connecting.");

                await base.OnConnectedAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var userId = UserConnection.GetUserConnectionByConnectionId(_userConnections, connectionId).UserId;
                Console.WriteLine("\nUser disconnected: " + userId);

                if (userId.Equals(null))
                {
                    Console.WriteLine("User disconnected but was not connected.");
                    return;
                }

                var connectedUserInformation = await _usersService.GetUserById(userId);

                UserConnection.RemoveConnectionId(_userConnections, connectionId);
                if (UserConnection.GetUserConnectionByUserId(_userConnections, userId).ConnectionIds.Count == 0)
                    UserConnection.RemoveUserId(_userConnections, userId);

                var message =
                    $"{connectedUserInformation?.Username} ({connectedUserInformation?.Name}) is now offline.";

                await SendUserConnectedNotification(userId, message);
                await UpdateUserOnlineFriends(userId);

                Console.WriteLine("User Disconnected: " + userId);

                await base.OnDisconnectedAsync(ex);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async Task Ping(string request)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                await AnswerPing(connectionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task UpdateCurrentlyPlaying(JsonObject? request)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var userConnection = UserConnection.GetUserConnectionByConnectionId(_userConnections, connectionId);
                // Get title field of json object
                Console.WriteLine($"\nUpdating currently playing {userConnection.UserId} {request?["title"]}");

                userConnection.CurrentlyPlaying = request;
                await UpdateUserOnlineFriends(userConnection.UserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Send Endpoints

        private async Task SendUserConnectedNotification(string userId, string message)
        {
            try
            {
                var userFriends = await _communityService.GetUserFriends(userId);

                var userKeys = UserConnection.GetUserIds(_userConnections);

                var userOnlineFriends = userFriends.Where(user => userKeys.Contains(user.Id));

                foreach (var friend in userOnlineFriends)
                {
                    foreach (var connectionId in UserConnection.GetUserConnectionByUserId(_userConnections, friend.Id)
                                 .ConnectionIds)
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
                Console.WriteLine("Updating user online friends.");
                foreach (var friend in userOnlineFriends)
                {
                    Console.WriteLine("Friend: " + friend.Id);

                    var friendOnlineFriends = await GetUserOnlineFriends(friend.Id);

                    var onlineFriendsDto = new List<LiveRoomUserDto>();
                    foreach (var onlineFriend in friendOnlineFriends)
                    {
                        var onlineFriendConnection =
                            UserConnection.GetUserConnectionByUserId(_userConnections, onlineFriend.Id);

                        onlineFriendsDto.Add(new LiveRoomUserDto(onlineFriend,
                            onlineFriendConnection.CurrentlyPlaying));
                    }


                    foreach (var connectionId in UserConnection.GetUserConnectionByUserId(_userConnections, friend.Id)
                                 .ConnectionIds)
                    {
                        await Clients.Client(connectionId).SendAsync("myOnlineFriends", onlineFriendsDto);
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
                var onlineFriendsDto = new List<LiveRoomUserDto>();
                foreach (var onlineFriend in userOnlineFriends)
                {
                    var onlineFriendConnection =
                        UserConnection.GetUserConnectionByUserId(_userConnections, onlineFriend.Id);

                    onlineFriendsDto.Add(new LiveRoomUserDto(onlineFriend,
                        onlineFriendConnection.CurrentlyPlaying));
                }

                Console.WriteLine("Updating User: " + userId);

                if (UserConnection.ContainsUserId(_userConnections,
                        userId)) // When following a user, check if he is connected before trying to update him
                {
                    foreach (var connectionId in UserConnection.GetUserConnectionByUserId(_userConnections, userId)
                                 .ConnectionIds)
                    {
                        await Clients.Client(connectionId).SendAsync("myOnlineFriends", onlineFriendsDto);
                    }
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
                var userOnlineFriends =
                    userFriends.Where(user => UserConnection.ContainsUserId(_userConnections, user.Id));
                return userOnlineFriends;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<UserDocument>();
            }
        }

        public async Task AnswerPing(string connectionId)
        {
            try
            {
                await Clients.Client(connectionId).SendAsync("ping", "pong");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}