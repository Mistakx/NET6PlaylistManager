using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.CommunityRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.UserResponses;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunityController : Controller
    {
        private readonly CommunityService _communityService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly UsersService _usersService;
        private readonly PlaylistRecommendationsService _playlistRecommendationsService;
        private readonly UserRecommendationsService _userRecommendationsService;
        private readonly PlaylistsService _playlistsService;
        private readonly SignalRService _signalRService;


        public CommunityController(
            CommunityService communityService,
            SessionTokensService sessionTokensService,
            UsersService usersService,
            PlaylistRecommendationsService playlistRecommendationsService,
            PlaylistsService playlistsService,
            UserRecommendationsService userRecommendationsService,
            SignalRService signalRService)
        {
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
            _usersService = usersService;
            _playlistRecommendationsService = playlistRecommendationsService;
            _playlistsService = playlistsService;
            _userRecommendationsService = userRecommendationsService;
            _signalRService = signalRService;
        }

        // READ

        [HttpPost("getFollowedPlaylists")]
        public async Task<List<PlaylistInformationDto>> GetFollowedPlaylists(GetFollowedPlaylistsDto request)
        {
            try
            {
                var playlistInformationDtoBuilder = new PlaylistInformationDtoBuilder();
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var followedPlaylistsDocuments = await _communityService.GetFollowedPlaylists(requestingUserId);
                if (followedPlaylistsDocuments == null) return new List<PlaylistInformationDto>();

                var followedPlaylistsInformation = new List<PlaylistInformationDto>();
                foreach (var followedPlaylistsDocument in followedPlaylistsDocuments)
                {
                    var playlistOwner = await _usersService.GetUserById(followedPlaylistsDocument.OwnerId);
                    var playlistViews =
                        await _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(
                            followedPlaylistsDocument.Id);
                    followedPlaylistsInformation.Add(playlistInformationDtoBuilder
                        .BeginBuilding(followedPlaylistsDocument.Id, followedPlaylistsDocument.Title,
                            followedPlaylistsDocument.Description, followedPlaylistsDocument.ThumbnailUrl,
                            followedPlaylistsDocument.ResultIds.Count)
                        .AddOwner(playlistOwner!).AddFollowing(true).AddViews(playlistViews).Build());
                }

                return followedPlaylistsInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<PlaylistInformationDto>();
            }
        }

        [HttpPost("getFollowedUsers")]
        public async Task<List<UserProfileDto>> GetFollowedUsers(GetFollowedUsersDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var followedUserDocuments = await _communityService.GetFollowedUsers(requestingUserId);
                if (followedUserDocuments == null) return new List<UserProfileDto>();

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followedUsersDocument in followedUserDocuments)
                {
                    var userViewablePlaylistsAmount =
                        await _playlistsService.GetPublicPlaylistsIds(followedUsersDocument.UserPlaylistIds);

                    var userViews =
                        await _userRecommendationsService.GetUserRecommendationsDocumentById(
                            followedUsersDocument.Id);

                    followedUsersInformation.Add(userProfileDtoBuilder
                        .BeginBuilding(followedUsersDocument, userViewablePlaylistsAmount.Count).AddFollowed(true)
                        .AddWeeklyViewsAmount(userViews).AddTotalViewsAmount(userViews).Build());
                }

                return followedUsersInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }

        [HttpPost("getUsersFollowingUser")]
        public async Task<List<UserProfileDto>> GetUsersFollowingUser(GetUsersFollowingUserDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return new List<UserProfileDto>();

                var followingUsersDocuments = await _communityService.GetUsersFollowingUser(requestedUser.Id);
                if (followingUsersDocuments == null) return new List<UserProfileDto>();

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followingUsersDocument in followingUsersDocuments)
                {
                    var playlistAlreadyBeingFollowed =
                        await _communityService.UserAlreadyBeingFollowed(requestingUserId, followingUsersDocument.Id);
                    followedUsersInformation.Add(userProfileDtoBuilder.BeginBuilding(followingUsersDocument)
                        .AddFollowed(playlistAlreadyBeingFollowed).Build());
                }

                return followedUsersInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }

        [HttpPost("getUsersFollowingPlaylist")]
        public async Task<List<UserProfileDto>> GetUsersFollowingPlaylist(GetUsersFollowingPlaylistDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedPlaylist = await _playlistsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return new List<UserProfileDto>();

                var followingUsersDocuments = await _communityService.GetUsersFollowingPlaylist(requestedPlaylist.Id);
                if (followingUsersDocuments == null) return new List<UserProfileDto>();

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followingUsersDocument in followingUsersDocuments)
                {
                    var playlistAlreadyBeingFollowed =
                        await _communityService.PlaylistAlreadyBeingFollowed(followingUsersDocument.Id,
                            requestingUserId);
                    followedUsersInformation.Add(userProfileDtoBuilder.BeginBuilding(followingUsersDocument)
                        .AddFollowed(playlistAlreadyBeingFollowed).Build());
                }

                return followedUsersInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }

        [HttpPost("getUsersUserFollows")]
        public async Task<List<UserProfileDto>> GetUsersUserFollow(GetUsersUserFollowsDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return new List<UserProfileDto>();

                var playlistIdsString = requestedUser.FollowingUsersIds.Select(p => p.ToString());
                var followedUsersDocuments = await _usersService.GetUsersByIds(playlistIdsString);
                if (followedUsersDocuments == null) return new List<UserProfileDto>();

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followingUsersDocument in followedUsersDocuments)
                {
                    var userAlreadyBeingFollowed =
                        await _communityService.UserAlreadyBeingFollowed(requestingUserId,
                            followingUsersDocument.Id);

                    followedUsersInformation.Add(userProfileDtoBuilder.BeginBuilding(followingUsersDocument)
                        .AddFollowed(userAlreadyBeingFollowed).Build());
                }

                return followedUsersInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }

        [HttpPost("getPlaylistsUserFollows")]
        public async Task<List<PlaylistInformationDto>> GetPlaylistsUserFollows(GetPlaylistsUserFollowsDto request)
        {
            try
            {
                var playlistInformationDtoBuilder = new PlaylistInformationDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return new List<PlaylistInformationDto>();

                var playlistIdsString = requestedUser.FollowingPlaylistsIds.Select(p => p.ToString());
                var followedPlaylistsIds = await _playlistsService.GetPlaylistsByIds(playlistIdsString);
                if (followedPlaylistsIds == null) return new List<PlaylistInformationDto>();

                var followedPlaylistsInformation = new List<PlaylistInformationDto>();
                foreach (var followingPlaylistDocument in followedPlaylistsIds)
                {
                    var playlistAlreadyBeingFollowed =
                        await _communityService.PlaylistAlreadyBeingFollowed(followingPlaylistDocument.Id,
                            requestingUserId);

                    followedPlaylistsInformation.Add(playlistInformationDtoBuilder
                        .BeginBuilding(followingPlaylistDocument).AddFollowing(playlistAlreadyBeingFollowed).Build());
                }

                return followedPlaylistsInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<PlaylistInformationDto>();
            }
        }


        // UPDATE

        [HttpPost("togglePlaylistFollow")]
        public async Task<IActionResult> TogglePlaylistFollow(TogglePlaylistFollowDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);

                var requestedPlaylist = await _playlistsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return BadRequest("Playlist not found");

                if (await _communityService.PlaylistAlreadyBeingFollowed(request.PlaylistId, requestingUserId))
                {
                    await _communityService.UnfollowPlaylist(requestingUserId, ObjectId.Parse(requestedPlaylist.Id));
                    return Ok("Successfully unfollowed playlist");
                }

                if (!await _communityService.PlaylistAlreadyBeingFollowed(request.PlaylistId, requestingUserId))
                {
                    await _communityService.FollowPlaylist(requestingUserId, ObjectId.Parse(requestedPlaylist.Id));
                    return Ok("Successfully followed playlist");
                }

                return BadRequest("Something went wrong while detecting if user already follows playlist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Unsuccessfully tried to follow/unfollow playlist");
            }
        }

        [HttpPost("toggleUserFollow")]
        public async Task<IActionResult> ToggleUserFollow(ToggleUserFollowDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return BadRequest("User not found");
                if (requestingUserId == requestedUser.Id) return BadRequest("You cannot follow yourself");

                if (await _communityService.UserAlreadyBeingFollowed(requestingUserId, requestedUser.Id))
                {
                    await _communityService.UnfollowUser(requestingUserId, ObjectId.Parse(requestedUser.Id));
                    await _signalRService.UpdateUser(requestingUserId);
                    await _signalRService.UpdateUser(requestedUser.Id);
                    return Ok("Successfully unfollowed user");
                }

                if (!await _communityService.UserAlreadyBeingFollowed(requestingUserId, requestedUser.Id))
                {
                    await _communityService.FollowUser(requestingUserId, new ObjectId(requestedUser.Id));
                    await _signalRService.UpdateUser(requestingUserId);
                    await _signalRService.UpdateUser(requestedUser.Id);
                    return Ok("Successfully followed user");
                }
                


                return BadRequest("Something went wrong while detecting if user already follows playlist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Unsuccessfully tried to follow/unfollow playlist");
            }
        }

        [HttpPost("sortUserFollow")]
        public async Task<IActionResult> SortUserFollow(SortUserFollowDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.followedUserUsername);
                if (requestedUser == null) return BadRequest("User not found");

                await _communityService.DeleteFollowedUserId(requestingUserId, ObjectId.Parse(requestedUser.Id));
                await _communityService.InsertFollowedUserIdInSpecificPosition(requestingUserId, request.NewIndex,
                    requestedUser.Id);
                return Ok("Successfully sorted followed user");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while sorting followed users");
            }
        }

        [HttpPost("sortPlaylistFollow")]
        public async Task<IActionResult> SortPlaylistFollow(SortPlaylistFollowDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedPlaylist = await _playlistsService.GetPlaylistById(request.FollowedPlaylistId);
                if (requestedPlaylist == null) return BadRequest("Playlist not found");

                await _communityService.DeleteFollowedPlaylistId(requestingUserId, new ObjectId(requestedPlaylist.Id));
                await _communityService.InsertFollowedPlaylistIdInSpecificPosition(requestingUserId, request.NewIndex,
                    requestedPlaylist.Id);
                return Ok("Successfully sorted followed playlist");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while sorting followed playlists");
            }
        }

        [HttpPost("removeFollowFromUser")]
        public async Task<IActionResult> RemoveFollowFromUser(RemoveFollowFromUserDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return BadRequest("User not found");

                await _communityService.DeleteFollowedUserId(requestedUser.Id, new ObjectId(requestingUserId));
                return Ok("Successfully removed follower");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while removing follower");
            }
        }

        [HttpPost("removeFollowFromPlaylist")] // TODO: Add security
        public async Task<IActionResult> RemoveFollowFromPlaylist(RemoveFollowFromPlaylistDto request)
        {
            try
            {
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return BadRequest("User not found");

                var requestedPlaylist = await _playlistsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return BadRequest("Playlist not found");

                await _communityService.DeleteFollowedPlaylistId(requestedUser.Id, new ObjectId(requestedPlaylist.Id));
                return Ok("Successfully removed follower");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while removing follower");
            }
        }
    }
}