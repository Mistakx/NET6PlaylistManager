using System.Collections;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;
using PlaylistBasicDetailsDto = SkyPlaylistManager.Models.DTOs.PlaylistResponses.PlaylistBasicDetailsDto;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendationsController : Controller
    {
        private readonly FilesManager _filesManager;
        private readonly GeneralizedResultFactory _generalizedResultFactory;
        private readonly GeneralizedResultsService _generalizedResultsService;
        private readonly PlaylistsService _playListsService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly UsersService _usersService;
        private readonly RecommendationsService _recommendationsService;


        public RecommendationsController(
            PlaylistsService playlistsService,
            UsersService usersService,
            GeneralizedResultsService generalizedResultsService,
            GeneralizedResultFactory generalizedResultFactory,
            SessionTokensService sessionTokensService,
            FilesManager filesManager,
            RecommendationsService recommendationsService
        )
        {
            _playListsService = playlistsService;
            _usersService = usersService;
            _generalizedResultsService = generalizedResultsService;
            _generalizedResultFactory = generalizedResultFactory;
            _sessionTokensService = sessionTokensService;
            _filesManager = filesManager;
            _recommendationsService = recommendationsService;
        }


        [HttpPost("saveView")]
        public async Task<IActionResult> SaveView(SaveViewDto request)
        {
            try
            {
                var recommendation = await _recommendationsService.GetResultInRecommended(
                    request.GeneralizedResult.Title,
                    request.GeneralizedResult.PlayerFactoryName, request.GeneralizedResult.PlatformPlayerUrl!);

                if (recommendation == null)
                {
                    await _recommendationsService.SaveView(request);
                }
                else
                {
                    await _recommendationsService.AddViewToResult(request.GeneralizedResult.Title,
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


        [HttpGet("getTrending")]
        public async Task<List<UnknownGeneralizedResultDto>?> GetTrending()
        {
            try
            {
                _recommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingResults = await _recommendationsService.GetTrending();

                var deserializedList = new List<UnknownGeneralizedResultDto>();
                foreach (var trendingResult in trendingResults)
                {
                    var deserializedResponse = BsonSerializer.Deserialize<GetTrendingDto>(trendingResult);
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

        [HttpPost("getViews")]
        public async Task<ReturnViewsDto?> GetViews(GetViewsDto request)
        {
            try
            {
                var views = await _recommendationsService.GetViews(
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
    }
}