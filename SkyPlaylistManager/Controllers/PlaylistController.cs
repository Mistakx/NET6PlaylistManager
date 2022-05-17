using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : Controller
    {
        private readonly PlaylistsService _playListsService;
        private readonly UsersService _usersService;
        private readonly GeneralizedResultsService _generalizedResultsService;
        private readonly MultimediaContentFactory _multimediaContentFactory;
        private readonly SessionTokensService _sessionTokensService;

        private const string PlaylistIdDoesntExistMessage = "Playlist ID doesn't exist.";

        public PlaylistController(
            PlaylistsService playlistsService,
            UsersService usersService,
            GeneralizedResultsService generalizedResultsService,
            MultimediaContentFactory multimediaContentFactory,
            SessionTokensService sessionTokensService
        )
        {
            _playListsService = playlistsService;
            _usersService = usersService;
            _generalizedResultsService = generalizedResultsService;
            _multimediaContentFactory = multimediaContentFactory;
            _sessionTokensService = sessionTokensService;
        }


        [HttpGet("getBasicDetails/{playlistId:length(24)}")] // TODO: Verificar se a playlist é privada. Só retornar a playlist caso seja pública ou partilhada com o user da sessão.
        public async Task<PlaylistBasicDetailsDto?> PlaylistBasicDetails(string playlistId)
        {
            var playlist = await _playListsService.GetPlaylistContents(playlistId);

            try
            {
                var deserializedPlaylist = BsonSerializer.Deserialize<PlaylistInformationWithContentsDto>(playlist);
                return deserializedPlaylist;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        [HttpGet("getGeneralizedResults{playlistId:length(24)}")] // TODO: Verificar se a playlist é privada. Só retornar a playlist caso seja pública ou partilhada com o user da sessão.
        public async Task<List<UnknownGeneralizedResultDto>?> PlaylistContent(string playlistId)
        {
            var playlist = await _playListsService.GetPlaylistContents(playlistId);

            try
            {
                var deserializedPlaylist = BsonSerializer.Deserialize<PlaylistInformationWithContentsDto>(playlist);
                return deserializedPlaylist;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        
        [HttpPost("addToPlaylist")]
        public async Task<IActionResult> AddMultimediaContentToPlaylist(JsonObject request)
        {
            try
            {
                var type = (string?) request["interface"];

                _multimediaContentFactory._args = request;
                var genericResult = _multimediaContentFactory[type!];
                await _generalizedResultsService.CreateGeneralizedResult(genericResult);

                var playlistId = (string?) request["playlistId"];
                var createdMultimediaContentId = ObjectId.Parse(genericResult.Id);

                await _playListsService.InsertMultimediaContentInPlaylist(playlistId!, createdMultimediaContentId);
                return Ok("Successfully added to playlist.");
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
                return Ok("Playlist successfully shared.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Ocorreu um erro na partilha da playlist.");
            }
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistDto request)
        {
            try
            {
                var playlist = new PlaylistCollection(request, _sessionTokensService);
                await _playListsService.CreatePlaylist(playlist);
                return Ok("Playlist successfully created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while creating playlist.");
            }
        }

        [HttpPost("editTitle")]
        public async Task<IActionResult> EditTitle(EditTitleDto title)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(title.Id!);

            if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);
            await _playListsService.UpdateTitle(title.Id!, title.NewTitle);

            return Ok("Playlist title successfully updated.");
        }

        [HttpPost("editDescription")]
        public async Task<IActionResult> EditDescription(EditDescriptionDto description)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(description.Id!);

            if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

            await _playListsService.UpdateDescription(description.Id!, description.NewDescription);
            return Ok("Playlist description successfully updated.");
        }

        [HttpPost("editVisibility")]
        public async Task<IActionResult> EditVisibility(EditVisibilityDto visibility)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(visibility.Id!);

            if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

            await _playListsService.UpdateVisibility(visibility.Id!, visibility.NewVisibility);
            return Ok("Playlist visibility successfully updated.");
        }

        [HttpPost("deletePlaylist")]
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDto playlist)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(playlist.Id!);


            if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

            var id = ObjectId.Parse(playlist.Id);
            await _playListsService.DeletePlaylist(playlist.Id!);

            var owner = foundPlaylist.Owner;
            await _usersService.DeleteUserPlaylist(owner, id);

            return Ok("Playlist successfully deleted.");
        }

        [HttpPost("removeShare")]
        public async Task<IActionResult> RemoveShare(PlaylistShareDto request)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
            if (foundPlaylist == null) return BadRequest(new {message = "No playlist with that ID exists."});

            var foundUser = await _usersService.GetUserById(request.UserId);
            if (foundUser == null) return BadRequest(new {message = "No user with that ID exists."});

            try
            {
                await _playListsService.DeleteShare(request.PlaylistId, new ObjectId(request.UserId));
                return Ok("User successfully removed.");
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
            var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId!);

            if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);
            
            try
            {
                var generalizedResultToDeleteId = new ObjectId(request.GeneralizedResultDatabaseId);
                await _playListsService.DeleteMultimediaContentInPlaylist(request.PlaylistId!,
                    generalizedResultToDeleteId);
                return Ok("Successfully removed from playlist.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Error while removing from playlist");
            }
        }
    }
}