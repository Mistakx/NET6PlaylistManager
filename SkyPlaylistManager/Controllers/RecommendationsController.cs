using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;
using PlaylistBasicDetailsDto = SkyPlaylistManager.Models.DTOs.PlaylistResponses.PlaylistBasicDetailsDto;

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

        public RecommendationsController(
            ContentRecommendationsService contentRecommendationsService,
            PlaylistRecommendationsService playlistRecommendationsService,
            UserRecommendationsService userRecommendationsService,
            UsersService usersService,
            PlaylistsService playlistsService)
        {
            _contentRecommendationsService = contentRecommendationsService;
            _playlistRecommendationsService = playlistRecommendationsService;
            _userRecommendationsService = userRecommendationsService;
            _userService = usersService;
            _playlistsService = playlistsService;
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

        [HttpPost("getContentViews")]
        public async Task<ReturnViewsDto?> GetContentViews(GetContentViewsDto request)
        {
            try
            {
                var views = await _contentRecommendationsService.GetViews(
                    request.PlayerFactoryName,
                    request.PlatformId,
                    request.PlatformPlayerUrl!
                );

                if (views == null) return new ReturnViewsDto(0, 0);

                var deserializedViews = BsonSerializer.Deserialize<ReturnViewsDto?>(views);
                return deserializedViews;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }


        //! Users
        [HttpPost("saveUserView")]
        public async Task<IActionResult> SaveUserView(SaveUserViewDto request)
        {
            try
            {
                var recommendation = await _userRecommendationsService.GetResultInRecommended(request.UserId);

                if (recommendation == null)
                {
                    await _userRecommendationsService.SaveView(request);
                }
                else
                {
                    await _userRecommendationsService.AddViewToResult(request.UserId,
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

        // [HttpGet("getTrendingUsers/{beginningOfUsername}")]
        // public async Task<List<UserBasicProfileDto>?> GetTrendingUser(string beginningOfUsername)
        // {
        //     try
        //     {
        //         _userRecommendationsService.UpdateRecommendationsWeeklyViews();
        //         var trendingUserIds = await _userRecommendationsService.GetTrendingUserIds(beginningOfUsername);
        //
        //         var deserializedUserIds = new List<string>();
        //         foreach (var trendingUserId in trendingUserIds)
        //         {
        //             var deserializedResponse = BsonSerializer.Deserialize<string>(trendingUserId);
        //             deserializedUserIds.Add(deserializedResponse);
        //         }
        //
        //         var deserializedUsersInformation = new List<UserBasicProfileDto>();
        //         foreach (var deserializedUserId in deserializedUserIds)
        //         {
        //             var userBasicDetails = await _userService.GetUserBasicDetails(deserializedUserId);
        //             var deserializedResponse = BsonSerializer.Deserialize<UserBasicProfileDto>(userBasicDetails);
        //             deserializedUsersInformation.Add(deserializedResponse);
        //         }
        //
        //         return deserializedUsersInformation;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.StackTrace);
        //         return null;
        //     }
        // }

        [HttpGet("getUserViews/{username}")]
        public async Task<ReturnViewsDto?> GetUserViews(string username)
        {
            try
            {
                var views = await _userRecommendationsService.GetViews(username);

                if (views == null) return new ReturnViewsDto(0, 0);

                var deserializedViews = BsonSerializer.Deserialize<ReturnViewsDto?>(views);
                return deserializedViews;
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
                var recommendation = await _playlistRecommendationsService.GetResultInRecommended(request.PlaylistId);

                if (recommendation == null)
                {
                    await _playlistRecommendationsService.SaveView(request);
                }
                else
                {
                    await _playlistRecommendationsService.AddViewToResult(request.PlaylistId,
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
        public async Task<List<PlaylistBasicDetailsDto>?> GetTrendingPlaylist(
            GetTrendingPlaylistsDto request)
        {
            try
            {
                _playlistRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingPlaylistIds =
                    await _playlistRecommendationsService.GetTrendingPlaylistIds(request.PlaylistName);

                var deserializedPlaylistIds = new List<string>();
                foreach (var trendingUserId in trendingPlaylistIds)
                {
                    var deserializedResponse = BsonSerializer.Deserialize<string>(trendingUserId);
                    deserializedPlaylistIds.Add(deserializedResponse);
                }

                var deserializedPlaylistsInformation = new List<PlaylistBasicDetailsDto>();
                foreach (var deserializedPlaylistId in deserializedPlaylistIds)
                {
                    var playlistBasicDetails = await _playlistsService.GetPlaylistDetails(deserializedPlaylistId);
                    var deserializedResponse =
                        BsonSerializer.Deserialize<PlaylistBasicDetailsDto>(playlistBasicDetails);
                    deserializedPlaylistsInformation.Add(deserializedResponse);
                }

                return deserializedPlaylistsInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        [HttpPost("getPlaylistViews")]
        public async Task<ReturnViewsDto?> GetPlaylistViews(GetPlaylistViewsDto request)
        {
            try
            {
                var views = await _playlistRecommendationsService.GetViews(request.PlaylistId);

                if (views == null) return new ReturnViewsDto(0, 0);

                var deserializedViews = BsonSerializer.Deserialize<ReturnViewsDto?>(views);
                return deserializedViews;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}