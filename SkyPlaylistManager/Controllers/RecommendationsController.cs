using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Controllers.Utils;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.ContentResponses;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendationsController : Controller
    {
        private readonly ContentRecommendationsService _contentRecommendationsService;
        private readonly PlaylistRecommendationsService _playlistRecommendationsService;
        private readonly UserRecommendationsService _userRecommendationsService;
        private readonly UsersService _userService;
        private readonly PlaylistsService _playlistsService;
        private readonly CommunityService _communityService;
        private readonly SessionTokensService _sessionTokensService;

        public RecommendationsController(
            ContentRecommendationsService contentRecommendationsService,
            PlaylistRecommendationsService playlistRecommendationsService,
            UserRecommendationsService userRecommendationsService,
            UsersService usersService,
            PlaylistsService playlistsService,
            CommunityService communityService,
            SessionTokensService sessionTokensService)
        {
            _contentRecommendationsService = contentRecommendationsService;
            _playlistRecommendationsService = playlistRecommendationsService;
            _userRecommendationsService = userRecommendationsService;
            _userService = usersService;
            _playlistsService = playlistsService;
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
        }


        //! Content
        [HttpPost("saveContentView")]
        public async Task<IActionResult> SaveContentView(SaveContentViewDto request)
        {
            try
            {
                var recommendation = await _contentRecommendationsService.GetContentInRecommendedCollection(
                    request.Content.Title,
                    request.Content.PlayerFactoryName, request.Content.PlatformPlayerUrl!);

                if (recommendation == null)
                {
                    await _contentRecommendationsService.SaveView(request);
                }
                else
                {
                    await _contentRecommendationsService.AddViewToResult(request.Content.Title,
                        request.Content.PlayerFactoryName, request.Content.PlatformPlayerUrl!,
                        recommendation.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingContent")]
        public async Task<List<UnknownContentResponseDto>?> GetTrendingContent(GetTrendingContentDto request)
        {
            try
            {
                var unknownContentResponseDtoBuilder = new UnknownContentResponseDtoBuilder();
                _contentRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingResults = await _contentRecommendationsService.GetTrendingContent(request.Limit);
                if (trendingResults == null) return new List<UnknownContentResponseDto>();

                var deserializedList = new List<UnknownContentResponseDto>();
                foreach (var trendingResult in trendingResults)
                {
                    deserializedList.Add(unknownContentResponseDtoBuilder
                        .BeginBuilding(trendingResult.GeneralizedResult).AddViews(trendingResult).Build());
                }

                return deserializedList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }


        //! Users
        [HttpPost("saveUserView")]
        public async Task<IActionResult?> SaveUserView(SaveUserViewDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);

                if (requestingUser?.Username == request.Username)
                    return BadRequest("Error: You cannot save a view your own profile");

                var requestedUser = await _userService.GetUserByUsername(request.Username);
                var recommendation =
                    await _userRecommendationsService.GetUserRecommendationsDocumentById(requestedUser?.Id!);

                if (recommendation == null)
                {
                    await _userRecommendationsService.SaveUserView(requestedUser?.Id!);
                }
                else
                {
                    await _userRecommendationsService.AddViewToUser(requestedUser?.Id!,
                        recommendation.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingUsers")]
        public async Task<List<UserProfileDto>> GetTrendingUsers(GetTrendingUsersDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return new List<UserProfileDto>();

                _userRecommendationsService.UpdateRecommendationsWeeklyViews();

                var trendingUsers =
                    await _userRecommendationsService.GetTrendingUsers(request.Username, request.Limit);
                if (trendingUsers == null) return new List<UserProfileDto>();

                var deserializedTrendingUsersInformation = new List<UserProfileDto>();
                foreach (var deserializedTrendingUser in trendingUsers)
                {
                    if (requestingUser?.Username != deserializedTrendingUser.User.Username)
                    {
                        var userViewablePlaylistsIds =
                            await _playlistsService.GetPublicPlaylistsIds(requestingUser?.UserPlaylistIds!);

                        var userViews =
                            await _userRecommendationsService.GetUserRecommendationsDocumentById(
                                deserializedTrendingUser.User.Id);

                        var deserializedTrendingUserIsBeingFollowedAlready =
                            await _communityService.UserAlreadyBeingFollowed(requestingUserId,
                                deserializedTrendingUser.User.Id);

                        deserializedTrendingUsersInformation.Add(userProfileDtoBuilder.BeginBuilding(
                                deserializedTrendingUser.User, userViewablePlaylistsIds.Count)
                            .AddWeeklyViewsAmount(userViews).AddTotalViewsAmount(userViews)
                            .AddFollowed(deserializedTrendingUserIsBeingFollowedAlready).Build());
                    }
                }

                // Need to find other users that are not in the trending list
                // There can be repeating users, so the query has to take the amount of trending users into account for the limit
                var allUsers = await _communityService.GetUsersByNameOrUsername(request.Username,
                    request.Limit + deserializedTrendingUsersInformation.Count);
                int amountOfUsersToFind;
                if (allUsers.Count > request.Limit)
                {
                    amountOfUsersToFind = request.Limit - deserializedTrendingUsersInformation.Count;
                }
                else
                {
                    amountOfUsersToFind = allUsers.Count;
                }

                for (var i = 0; i < amountOfUsersToFind; i++)
                {
                    var userViewablePlaylistsAmount =
                        await _playlistsService.GetPublicPlaylistsIds(allUsers.ElementAt(i).UserPlaylistIds!);

                    var currentUserViews = await
                        _userRecommendationsService.GetUserRecommendationsDocumentById(allUsers.ElementAt(i).Id);

                    var currentUserIsBeingFollowed = await
                        _communityService.UserAlreadyBeingFollowed(requestingUserId, allUsers.ElementAt(i).Id);

                    var currentUser = userProfileDtoBuilder
                        .BeginBuilding(allUsers.ElementAt(i), userViewablePlaylistsAmount.Count)
                        .AddFollowed(currentUserIsBeingFollowed).AddWeeklyViewsAmount(currentUserViews)
                        .AddTotalViewsAmount(currentUserViews).Build();

                    if (requestingUser?.Username != currentUser.Username)
                    {
                        deserializedTrendingUsersInformation.Add(currentUser);
                    }
                }

                return deserializedTrendingUsersInformation.GroupBy(userProfile => userProfile.Username)
                    .Select(group => group.First()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }


        //! Playlists
        [HttpPost("savePlaylistView")]
        public async Task<IActionResult> SavePlaylistView(SavePlaylistViewDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return BadRequest("Error: User not found");

                var playlistRecommendationDocument =
                    await _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(request.PlaylistId);


                foreach (var userPlaylistId in requestingUser.UserPlaylistIds)
                {
                    if (userPlaylistId == new ObjectId(request.PlaylistId))
                    {
                        return BadRequest("Error: You cannot save a view to a playlist you own");
                    }
                }

                if (playlistRecommendationDocument == null)
                {
                    await _playlistRecommendationsService.SaveNewPlaylistView(request.PlaylistId);
                }
                else
                {
                    await _playlistRecommendationsService.AddViewToPlaylist(request.PlaylistId,
                        playlistRecommendationDocument.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingPlaylists")]
        public async Task<List<PlaylistInformationDto>> GetTrendingPlaylists(GetTrendingPlaylistsDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return new List<PlaylistInformationDto>();
                var playlistInformationDtoBuilder = new PlaylistInformationDtoBuilder();

                _playlistRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingPlaylistDocuments =
                    await _playlistRecommendationsService.GetTrendingPlaylists(request.PlaylistName, request.Limit);
                if (trendingPlaylistDocuments == null) return new List<PlaylistInformationDto>();

                var deserializedTrendingPlaylistsInformation = new List<PlaylistInformationDto>();
                foreach (var trendingPlaylistDocument in trendingPlaylistDocuments)
                {
                    if (!RecommendationUtils.PlaylistBelongsToRequestingUser(trendingPlaylistDocument.Playlist,
                            requestingUser) &&
                        trendingPlaylistDocument.Playlist.Visibility == "Public")
                    {
                        var requestingUserFollowsPlaylist =
                            await _communityService.PlaylistAlreadyBeingFollowed(trendingPlaylistDocument.PlaylistId,
                                requestingUserId);

                        var playlistOwner = await _userService.GetUserById(trendingPlaylistDocument.Playlist.OwnerId);

                        deserializedTrendingPlaylistsInformation.Add(playlistInformationDtoBuilder.BeginBuilding(
                                trendingPlaylistDocument.Playlist.Id,
                                trendingPlaylistDocument.Playlist.Title,
                                trendingPlaylistDocument.Playlist.Description,
                                trendingPlaylistDocument.Playlist.ThumbnailUrl,
                                trendingPlaylistDocument.Playlist.ResultIds.Count)
                            .AddViews(trendingPlaylistDocument).AddFollowing(requestingUserFollowsPlaylist)
                            .AddOwner(playlistOwner!)
                            .Build());
                    }
                }

                // Need to find other playlists that are not in the trending list
                // There can be repeating playlists, so the query has to take the amount of trending playlists into account for the limit
                var allPlaylists = await _communityService.GetPlaylistsByTitle(request.PlaylistName,
                    request.Limit + deserializedTrendingPlaylistsInformation.Count);
                int amountOfPlaylistsToFind;
                if (allPlaylists.Count > request.Limit)
                    amountOfPlaylistsToFind = request.Limit - deserializedTrendingPlaylistsInformation.Count;
                else amountOfPlaylistsToFind = allPlaylists.Count;

                for (var i = 0; i < amountOfPlaylistsToFind; i++)
                {
                    if (!RecommendationUtils.PlaylistBelongsToRequestingUser(allPlaylists.ElementAt(i),
                            requestingUser) &&
                        allPlaylists.ElementAt(i).Visibility == "Public")
                    {
                        var currentPlaylistViews = await
                            _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(allPlaylists
                                .ElementAt(i)
                                .Id);

                        var requestingUserFollowsPlaylist =
                            await _communityService.PlaylistAlreadyBeingFollowed(allPlaylists.ElementAt(i).Id,
                                requestingUserId);

                        var playlistOwner = await _userService.GetUserById(allPlaylists.ElementAt(i).OwnerId);

                        var currentPlaylistInformation = playlistInformationDtoBuilder.BeginBuilding(
                                allPlaylists.ElementAt(i).Id,
                                allPlaylists.ElementAt(i).Title,
                                allPlaylists.ElementAt(i).Description,
                                allPlaylists.ElementAt(i).ThumbnailUrl, allPlaylists.ElementAt(i).ResultIds.Count)
                            .AddViews(currentPlaylistViews!)
                            .AddFollowing(requestingUserFollowsPlaylist).AddOwner(playlistOwner!).Build();

                        deserializedTrendingPlaylistsInformation.Add(currentPlaylistInformation);
                    }
                }

                return deserializedTrendingPlaylistsInformation.GroupBy(playlist => playlist.Id)
                    .Select(group => group.First()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<PlaylistInformationDto>();
            }
        }
    }
}