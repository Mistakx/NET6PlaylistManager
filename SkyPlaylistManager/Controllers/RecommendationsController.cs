using System.Collections;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
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
                    await _recommendationsService.AddViewToResultInRecommended(request.GeneralizedResult.Title,
                        request.GeneralizedResult.PlayerFactoryName, request.GeneralizedResult.PlatformPlayerUrl!,
                        recommendation.ViewsAmount);
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
        public async  Task<List<RecommendationsDocument>?> GetTrending()
        {
            try
            {
                _recommendationsService.UpdateRecommendationsViews();
                return await _recommendationsService.GetTrending();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}