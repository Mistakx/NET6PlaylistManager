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
using PlaylistBasicDetailsDto = SkyPlaylistManager.Models.DTOs.PlaylistResponses.PlaylistBasicDetailsDto;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : Controller
    {
        private const string PlaylistIdDoesntExistMessage = "Playlist ID doesn't exist";
        private readonly FilesManager _filesManager;
        private readonly GeneralizedResultFactory _generalizedResultFactory;
        private readonly GeneralizedResultsService _generalizedResultsService;
        private readonly PlaylistsService _playListsService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly UsersService _usersService;

        public PlaylistController(
            PlaylistsService playlistsService,
            UsersService usersService,
            GeneralizedResultsService generalizedResultsService,
            GeneralizedResultFactory generalizedResultFactory,
            SessionTokensService sessionTokensService,
            FilesManager filesManager
        )
        {
            _playListsService = playlistsService;
            _usersService = usersService;
            _generalizedResultsService = generalizedResultsService;
            _generalizedResultFactory = generalizedResultFactory;
            _sessionTokensService = sessionTokensService;
            _filesManager = filesManager;
        }


        [HttpGet(
            "getBasicDetails/{playlistId:length(24)}")] // TODO: Verificar se a playlist é privada. Só retornar a playlist caso seja pública ou partilhada com o user da sessão.
        public async Task<PlaylistBasicDetailsDto?> PlaylistBasicDetails(string playlistId)
        {
            var basicDetails = await _playListsService.GetPlaylistDetails(playlistId);

            try
            {
                var deserializedBasicDetails = BsonSerializer.Deserialize<PlaylistBasicDetailsDto>(basicDetails);
                return deserializedBasicDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        [HttpGet(
            "getGeneralizedResults/{playlistId:length(24)}")] // TODO: Verificar se a playlist é privada. Só retornar a playlist caso seja pública ou partilhada com o user da sessão.
        public async Task<ArrayList?> PlaylistContent(string playlistId)
        {

            try
            {
                var playlistResults = await _playListsService.GetPlaylistGeneralizedResults(playlistId);
                var deserializedPlaylistResults = BsonSerializer.Deserialize<PlaylistResultsDto>(playlistResults);

                var playlistResultsOrderedIds = await _playListsService.GetPlaylistContentOrderedIds(playlistId);
                var deserializedPlaylistResultsOrderedIds = BsonSerializer.Deserialize<PlaylistContentsOrderedIdsDto>(playlistResultsOrderedIds);

                var orderedPlaylistResults = new ArrayList();
                foreach (var playlistContentId in deserializedPlaylistResultsOrderedIds.ResultIds)
                {
                    foreach (var playlistContent in deserializedPlaylistResults.Results)
                    {
                        if (playlistContent.DatabaseId == playlistContentId.ToString())
                        {
                            orderedPlaylistResults.Add(playlistContent);
                        }
                    }
                }
                return orderedPlaylistResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistDto request)
        {
            try
            {
                var userId = _sessionTokensService.GetUserId(request.SessionToken);
                var playlist = new PlaylistDocument(request, _sessionTokensService);
                await _playListsService.CreatePlaylist(playlist, userId);
                return Ok("Playlist successfully created");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while creating playlist");
            }
        }

        [HttpPost("deletePlaylist")]
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDto request)
        {
            try
            {
                var foundPlaylist = await _playListsService.GetPlaylistById(request.Id);


                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                var id = ObjectId.Parse(request.Id);
                await _playListsService.DeletePlaylist(request.Id);

                var owner = foundPlaylist.Owner;
                await _usersService.DeleteUserPlaylist(owner, id);

                return Ok("Playlist successfully deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while deleting playlist");
            }
        }
        
        [HttpPost("addToPlaylist")]
        public async Task<IActionResult> AddGeneralizedResultToPlaylist(AddGeneralizedResultPlaylistDto request)
        {
            try
            {
                if (await _playListsService.GeneralizedResultAlreadyInPlaylist(
                        request.PlaylistId,
                        request.Title,
                        request.PlayerFactoryName,
                        request.PlatformPlayerUrl!
                    ))
                {
                    return BadRequest("Result already in playlist");
                }

                _generalizedResultFactory.Request = request;
                var generalizedResult = _generalizedResultFactory[request.ResultType];
                await _generalizedResultsService.CreateGeneralizedResult(generalizedResult);

                var generalizedResultId = ObjectId.Parse(generalizedResult.Id);

                await _playListsService.InsertGeneralizedResultInPlaylist(request.PlaylistId, generalizedResultId);
                return Ok("Successfully added to playlist");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while adding to playlist");
            }
        }

        [HttpPost("sortResult")]
        public async Task<IActionResult> SortResult(SortPlaylistResultsDto request)
        {
            try
            {
                var generalResultId = ObjectId.Parse(request.GeneralizedResultDatabaseId);
                await _playListsService.DeleteMultimediaContentInPlaylist(request.PlaylistId, generalResultId);
                await _playListsService.InsertGeneralizedResultInSpecificPosition(request);
                return Ok("Successfully sorted result");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while sorting result");
            }
        }

        [HttpPost("sortPlaylist")]
        public async Task<IActionResult> SortResult(SortPlaylistsDto request)
        {
            try
            {
                var userId = _sessionTokensService.GetUserId(request.SessionToken);
                
                await _playListsService.DeletePlaylistInUser(userId, new ObjectId(request.PlaylistId));
                await _playListsService.InsertPlaylistInSpecificPosition(request.PlaylistId, request.NewIndex, userId);
                return Ok("Successfully sorted playlist");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while sorting playlist");
            }
        }

        
        [HttpPost("deleteGeneralizedResult")]
        public async Task<IActionResult> DeleteGeneralizedResult(DeletePlaylistContentDto request)
        {
            try
            {
                var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);

                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                var generalizedResultToDeleteId = new ObjectId(request.GeneralizedResultDatabaseId);
                await _playListsService.DeleteMultimediaContentInPlaylist(request.PlaylistId,
                    generalizedResultToDeleteId);
                return Ok("Successfully removed from playlist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Error while removing from playlist");
            }
        }

        [HttpPost("editPlaylistPhoto")]
        public async Task<IActionResult> EditProfilePhoto([FromForm] EditPlaylistThumbnail request)
        {
            if (!_filesManager.IsValidImage(request.PlaylistPhoto)) return BadRequest("Invalid image format");
            try
            {
                var playlistId = request.PlaylistId;
                var generatedFileName = _filesManager.InsertInDirectory(request.PlaylistPhoto, "PlaylistsThumbnails");

                var oldPhoto = await _playListsService.GetPlaylistPhoto(playlistId);

                await _playListsService.UpdatePlaylistPhoto(playlistId,
                    "GetImage/PlaylistsThumbnails/" + generatedFileName);
                _filesManager.DeleteFromDirectory((string) oldPhoto["thumbnailUrl"], "PlaylistsThumbnails");
                return Ok("GetImage/PlaylistsThumbnails/" + generatedFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occured while changing profile picture");
            }
        }

        [HttpPost("setCoverItem")]
        public async Task<IActionResult> SetCoverItem(SetCoverItem request)
        {
            try
            {
                var playlistId = request.PlaylistId;

                var oldPhoto = await _playListsService.GetPlaylistPhoto(playlistId);

                await _playListsService.UpdatePlaylistPhoto(playlistId, request.CoverUrl);
                try
                {
                    _filesManager.DeleteFromDirectory((string) oldPhoto["thumbnailUrl"], "PlaylistsThumbnails");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return Ok("Successfully set cover photo");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occured while changing cover photo");
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditPlaylist(EditPlaylistDto request)
        {
            try
            {
                await _playListsService.UpdatePlaylist(request);
                return Ok("Playlist successfully edited");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while editing playlist");
            }
        }
    }
}