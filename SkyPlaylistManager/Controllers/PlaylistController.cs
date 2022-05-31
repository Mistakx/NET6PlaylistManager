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
        public async Task<PlaylistContentsDto?> PlaylistContent(string playlistId)
        {
            var playlistContents = await _playListsService.GetPlaylistGeneralizedResults(playlistId);

            try
            {
                var deserializedPlaylistContents = BsonSerializer.Deserialize<PlaylistContentsDto>(playlistContents);
                return deserializedPlaylistContents;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }


        [HttpPost("addToPlaylist")]
        public async Task<IActionResult> AddGeneralizedResultToPlaylist(JsonObject request)
            // TODO: Check if content is already in playlist 
        {
            try
            {
                var resultType = (string?) request["resultType"];

                _generalizedResultFactory.Request = request;
                var generalizedResult = _generalizedResultFactory[resultType!];
                await _generalizedResultsService.CreateGeneralizedResult(generalizedResult);

                var playlistId = (string?) request["playlistId"];
                var generalizedResultId = ObjectId.Parse(generalizedResult.Id);

                await _playListsService.InsertGeneralizedResultInPlaylist(playlistId!, generalizedResultId);
                return Ok("Successfully added to playlist");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return NotFound();
            }
        }


        [HttpPost("share")]
        public async Task<IActionResult> SharePlaylist(PlaylistShareDto newPlaylistShareDto)
        {
            try
            {
                await _playListsService.InsertUserInSharedWithArray(newPlaylistShareDto.PlaylistId,
                    new ObjectId(newPlaylistShareDto.UserId));
                return Ok("Playlist successfully shared");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("An error occurred while sharing the playlist");
            }
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistDto request)
        {
            try
            {
                var playlist = new PlaylistDocument(request, _sessionTokensService);
                await _playListsService.CreatePlaylist(playlist);
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

        [HttpPost("removeShare")]
        public async Task<IActionResult> RemoveShare(PlaylistShareDto request)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
            if (foundPlaylist == null) return BadRequest(new {message = "No playlist with that ID exists"});

            var foundUser = await _usersService.GetUserById(request.UserId);
            if (foundUser == null) return BadRequest(new {message = "No user with that ID exists"});

            try
            {
                await _playListsService.DeleteShare(request.PlaylistId, new ObjectId(request.UserId));
                return Ok("User successfully removed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return NotFound();
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