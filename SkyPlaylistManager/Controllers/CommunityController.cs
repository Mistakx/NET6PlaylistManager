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


        public CommunityController(
            CommunityService communityService,
            SessionTokensService sessionTokensService,
            UsersService usersService,
            PlaylistRecommendationsService playlistRecommendationsService,
            PlaylistsService playlistsService,
            UserRecommendationsService userRecommendationsService)
        {
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
            _usersService = usersService;
            _playlistRecommendationsService = playlistRecommendationsService;
            _playlistsService = playlistsService;
            _userRecommendationsService = userRecommendationsService;
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
                    var userPlaylistsWeeklyViews =
                        await _playlistRecommendationsService.GetUserWeeklyPlaylistViews(followedUsersDocument
                            .UserPlaylistIds);

                    var userPlaylistsTotalView =
                        await _playlistRecommendationsService.GetUserTotalPlaylistViews(followedUsersDocument
                            .UserPlaylistIds);

                    var userPlaylistsItemsAmount =
                        await _playlistsService.GetTotalContentInPlaylists(followedUsersDocument.UserPlaylistIds);
                    
                    var userViews =
                        await _userRecommendationsService.GetUserRecommendationsDocumentById(
                            followedUsersDocument.Id);

                    followedUsersInformation.Add(userProfileDtoBuilder
                        .BeginBuilding(followedUsersDocument, userPlaylistsWeeklyViews, userPlaylistsTotalView,
                            userPlaylistsItemsAmount, userViews).AddFollowed(true).Build());
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
                var requestedUser = await _usersService.GetUserByUsername(request.Username);
                if (requestedUser == null) return new List<UserProfileDto>();

                var followingUsersDocuments = new List<UserDocument?>();
                foreach (var userId in requestedUser.UsersFollowingIds)
                {
                    followingUsersDocuments.Add(await _usersService.GetUserById(userId.ToString()));
                }

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followingUsersDocument in followingUsersDocuments)
                {
                    followedUsersInformation.Add(new UserProfileDto(followingUsersDocument!));
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
                var requestedPlaylist = await _playlistsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return new List<UserProfileDto>();

                var followingUsersDocuments = new List<UserDocument?>();
                foreach (var userId in requestedPlaylist.UsersFollowingIds)
                {
                    followingUsersDocuments.Add(await _usersService.GetUserById(userId.ToString()));
                }

                var followedUsersInformation = new List<UserProfileDto>();
                foreach (var followingUsersDocument in followingUsersDocuments)
                {
                    followedUsersInformation.Add(new UserProfileDto(followingUsersDocument!));
                }

                return followedUsersInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
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
                    await _communityService.UnfollowPlaylist(requestedPlaylist.Id, new ObjectId(requestingUserId));
                    return Ok("Successfully unfollowed playlist");
                }

                if (!await _communityService.PlaylistAlreadyBeingFollowed(request.PlaylistId, requestingUserId))
                {
                    await _communityService.FollowPlaylist(requestedPlaylist.Id, new ObjectId(requestingUserId));
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

                if (await _communityService.UserAlreadyBeingFollowed(requestedUser.Id, requestingUserId))
                {
                    await _communityService.UnfollowUser(requestedUser.Id, new ObjectId(requestingUserId));
                    return Ok("Successfully unfollowed user");
                }

                if (!await _communityService.UserAlreadyBeingFollowed(requestedUser.Id, requestingUserId))
                {
                    await _communityService.FollowUser(requestedUser.Id, new ObjectId(requestingUserId));
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
    }
}