using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.Database.GenericResults;
using SkyPlaylistManager.Models.DTOs;
using SkyPlaylistManager.Models.DTOs.Playlist;

namespace SkyPlaylistManager.Controllers
{


    [ApiController]
    [Route("[controller]")] // "[controller]" will define the route as /Playlist
    public class PlaylistController : Controller
    {
        private readonly PlaylistsService _playListsService;
        private readonly MultimediaContentsService _multimediaContentsService;
        private readonly MultimediaContentFactory _multimediaContentFactory;

        public PlaylistController(PlaylistsService playlistsService, MultimediaContentsService multimediaContentsService, 
            MultimediaContentFactory multimediaContentFactory)
        {
            _playListsService = playlistsService;
            _multimediaContentsService = multimediaContentsService;
            _multimediaContentFactory = multimediaContentFactory;
        }



        [HttpGet("{playlistId:length(24)}")] // TODO: Verificar se a playlist é privada. Só retornar a playlist caso seja pública ou partilhada com o user da sessão.
        public async Task<PlaylistAndContentsDto?> PlaylistContent(string playlistId)
        {
            var playlist = await _playListsService.GetPlaylistContents(playlistId);
            
            try
            {
                var deserializedPlaylist = BsonSerializer.Deserialize<PlaylistAndContentsDto>(playlist);
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
             GenericResult genericResult;
           
            try
            {
                string? type = (string?) request["interface"];

                _multimediaContentFactory._args = request;
                genericResult = _multimediaContentFactory[type];
                await _multimediaContentsService.CreateMultimediaContent(genericResult);

                string? playlistId = (string?) request["playlistId"];
                ObjectId createdMultimediaContentId = ObjectId.Parse(genericResult.Id);

                await _playListsService.InsertMultimediaContentInPlaylist(playlistId, createdMultimediaContentId);
                return Ok(genericResult);

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
                await _playListsService.InsertUserInSharedWithArray(newPlaylistShareDto.PlaylistID, new ObjectId(newPlaylistShareDto.UserID));
                return Ok("Playlist partilhada.");
                //return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Ocorreu um erro na partilha da playlist.");
            }
        }

    

        [HttpPost("create")]
        public async Task<IActionResult> CreatePlaylist(NewPlaylistDto newPlaylist)
        {
            var playlist = new PlaylistCollection(newPlaylist);
            
                try
                {
                    await _playListsService.CreatePlaylist(playlist);
                    return Ok("Playlist criada.");
                    //return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return BadRequest("Ocorreu um erro na criação da playlist.");
                }
        }


    }
}
