using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
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
                var recommendation = await _contentRecommendationsService.GetResultInRecommended(
                    request.GeneralizedResult.Title,
                    request.GeneralizedResult.PlayerFactoryName, request.GeneralizedResult.PlatformPlayerUrl!);

                if (recommendation == null)
                {
                    await _contentRecommendationsService.SaveView(request);
                }
                else
                {
                    await _contentRecommendationsService.AddViewToResult(request.GeneralizedResult.Title,
                        request.GeneralizedResult.PlayerFactoryName, request.GeneralizedResult.PlatformPlayerUrl!,
                        recommendation.WeeklyViewsAmount, recommendation.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpGet("getTrendingContent")]
        public async Task<List<UnknownGeneralizedResultDto>?> GetTrendingContent()
        {
            try
            {
                _contentRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingResults = await _contentRecommendationsService.GetTrending();

                var deserializedList = new List<UnknownGeneralizedResultDto>();
                foreach (var trendingResult in trendingResults)
                {
                    var deserializedResponse = BsonSerializer.Deserialize<GetTrendingContentDto>(trendingResult);
                    deserializedList.Add(deserializedResponse.generalizedResult);
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
                var recommendation = await _userRecommendationsService.GetUserRecommendationsDocumentById(requestedUser?.Id!);

                if (recommendation == null)
                {
                    await _userRecommendationsService.SaveUserView(requestedUser?.Id!);
                }
                else
                {
                    await _userRecommendationsService.AddViewToUser(requestedUser?.Id!,
                        recommendation.WeeklyViewsAmount, recommendation.TotalViewsAmount);
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
        public async Task<List<UserProfileDto>?> GetTrendingUsers(GetTrendingUsersDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);

                _userRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingUsers =
                    await _userRecommendationsService.GetTrendingUsers(request.Username, request.Limit);

                if (trendingUsers == null) return null;

                var deserializedTrendingUsers = new List<GetTrendingUsersLookupDto>();
                foreach (var trendingUser in trendingUsers)
                {
                    var deserializedTrendingUser = BsonSerializer.Deserialize<GetTrendingUsersLookupDto>(trendingUser);
                    deserializedTrendingUsers.Add(deserializedTrendingUser);
                }

                var deserializedTrendingUsersInformation = new List<UserProfileDto>();
                foreach (var deserializedTrendingUser in deserializedTrendingUsers)
                {
                    if (requestingUser?.Username != deserializedTrendingUser.User.Username)
                        deserializedTrendingUsersInformation.Add(new UserProfileDto(
                            deserializedTrendingUser.User.Name, deserializedTrendingUser.User.Username,
                            deserializedTrendingUser.User.ProfilePhotoUrl, deserializedTrendingUser.WeeklyViewsAmount,
                            deserializedTrendingUser.TotalViewsAmount));
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
                    var currentUserViews = await
                        _userRecommendationsService.GetUserRecommendationsDocumentById(allUsers.ElementAt(i).Id);

                    int weeklyViews;
                    int totalViews;
                    if (currentUserViews != null)
                    {
                        weeklyViews = currentUserViews.WeeklyViewsAmount;
                        totalViews = currentUserViews.TotalViewsAmount;
                    }
                    else
                    {
                        weeklyViews = 0;
                        totalViews = 0;
                    }

                    var currentUser = new UserProfileDto(allUsers.ElementAt(i).Name,
                        allUsers.ElementAt(i).Username, allUsers.ElementAt(i).ProfilePhotoUrl,
                        weeklyViews, totalViews);

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
                return null;
            }
        }

        //! Playlists
        [HttpPost("savePlaylistView")]
        public async Task<IActionResult> SavePlaylistView(SavePlaylistViewDto request)
        {
            try
            {
                var recommendation =
                    await _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(request.PlaylistId);

                if (recommendation == null)
                {
                    await _playlistRecommendationsService.SaveNewPlaylistView(request);
                }
                else
                {
                    await _playlistRecommendationsService.AddViewToPlaylist(request.PlaylistId,
                        recommendation.WeeklyViewsAmount, recommendation.TotalViewsAmount);
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
        public async Task<List<PlaylistDto>?> GetTrendingPlaylists(GetTrendingPlaylistsDto request)
        {
            bool PlaylistBelongsToRequestingUser(PlaylistDocument playlist, UserDocument user)
            {
                foreach (var userPlaylistId in user.UserPlaylistIds)
                {
                    if (new ObjectId(playlist.Id) == userPlaylistId)
                    {
                        return true;
                    }
                }

                return false;
            }

            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return null;

                _playlistRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingPlaylists =
                    await _playlistRecommendationsService.GetTrendingPlaylists(request.PlaylistName, request.Limit);
                if (trendingPlaylists == null) return null;

                var deserializedTrendingPlaylists = new List<GetTrendingPlaylistsLookupDto>();
                foreach (var trendingPlaylist in trendingPlaylists)
                {
                    var deserializedTrendingPlaylist =
                        BsonSerializer.Deserialize<GetTrendingPlaylistsLookupDto>(trendingPlaylist);
                    deserializedTrendingPlaylists.Add(deserializedTrendingPlaylist);
                }

                var deserializedTrendingPlaylistsInformation = new List<PlaylistDto>();
                foreach (var deserializedTrendingPlaylist in deserializedTrendingPlaylists)
                {
                    if (!PlaylistBelongsToRequestingUser(deserializedTrendingPlaylist.Playlist, requestingUser))
                        deserializedTrendingPlaylistsInformation.Add(new PlaylistDto(
                            deserializedTrendingPlaylist.Playlist.Id, deserializedTrendingPlaylist.Playlist.Title,
                            deserializedTrendingPlaylist.Playlist.Visibility,
                            deserializedTrendingPlaylist.Playlist.Description,
                            deserializedTrendingPlaylist.Playlist.ThumbnailUrl,
                            deserializedTrendingPlaylist.Playlist.ResultsAmount,
                            deserializedTrendingPlaylist.WeeklyViewsAmount,
                            deserializedTrendingPlaylist.TotalViewsAmount));
                }

                // Need to find other playlists that are not in the trending list
                // There can be repeating playlists, so the query has to take the amount of trending playlists into account for the limit
                var allPlaylists = await _communityService.GetPlaylistsByTitle(request.PlaylistName,
                    request.Limit + deserializedTrendingPlaylistsInformation.Count);
                int amountOfPlaylistsToFind;
                if (allPlaylists.Count > request.Limit)
                {
                    amountOfPlaylistsToFind = request.Limit - deserializedTrendingPlaylistsInformation.Count;
                }
                else
                {
                    amountOfPlaylistsToFind = allPlaylists.Count;
                }

                for (var i = 0; i < amountOfPlaylistsToFind; i++)
                {
                    var currentPlaylistViews = await
                        _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(allPlaylists.ElementAt(i)
                            .Id);

                    int weeklyViews;
                    int totalViews;
                    if (currentPlaylistViews != null)
                    {
                        weeklyViews = currentPlaylistViews.WeeklyViewsAmount;
                        totalViews = currentPlaylistViews.TotalViewsAmount;
                    }
                    else
                    {
                        weeklyViews = 0;
                        totalViews = 0;
                    }

                    var currentPlaylist = new PlaylistDto(allPlaylists.ElementAt(i).Id,
                        allPlaylists.ElementAt(i).Title, allPlaylists.ElementAt(i).Visibility,
                        allPlaylists.ElementAt(i).Description,
                        allPlaylists.ElementAt(i).ThumbnailUrl, allPlaylists.ElementAt(i).ResultsAmount,
                        weeklyViews, totalViews);

                    if (!PlaylistBelongsToRequestingUser(allPlaylists.ElementAt(i), requestingUser))
                    {
                        deserializedTrendingPlaylistsInformation.Add(currentPlaylist);
                    }
                }

                return deserializedTrendingPlaylistsInformation.GroupBy(playlist => playlist.Id)
                    .Select(group => group.First()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}