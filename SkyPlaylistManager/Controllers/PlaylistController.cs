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
        private readonly UsersService _usersService;
        private readonly MultimediaContentsService _multimediaContentsService;
        private readonly MultimediaContentFactory _multimediaContentFactory;

        public PlaylistController(PlaylistsService playlistsService,UsersService usersService, MultimediaContentsService multimediaContentsService, 
            MultimediaContentFactory multimediaContentFactory)
        {
            _playListsService = playlistsService;
            _usersService = usersService;
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
        
        [HttpPost("editTitle")]
        public async Task<IActionResult> EditTitle(EditTitleDTO title)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(title.Id);

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                await _playListsService.UpdateTitle(title.Id, title.NewTitle);

                return Ok("O título da playlist foi atualizado com sucesso.");
            }
        }
        
        [HttpPost("editDescription")]
        public async Task<IActionResult> EditDescription(EditDescriptionDTO description)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(description.Id);

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                await _playListsService.UpdateDescription(description.Id, description.NewDescription);
                return Ok("A descrição da playlist foi atualizada com sucesso.");
            }
        }
        
        [HttpPost("editVisibility")]
        public async Task<IActionResult> EditVisibility(EditVisibilityDTO visibility)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(visibility.Id);

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                await _playListsService.UpdateVisibility(visibility.Id, visibility.NewVisibility);
                return Ok("A visibilidade da playlist foi atualizada com sucesso.");
            }
        }
        
        [HttpPost("deletePlaylist")]
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDTO playlist)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(playlist.Id);
            

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                string Owner = foundPlaylist.Owner;
                ObjectId id = ObjectId.Parse(playlist.Id);
                
                await _playListsService.DeletePlaylist(playlist.Id);
                await _usersService.DeleteUserPlaylist(Owner, id);

                return Ok("A playlist selecionada foi removida.");
            }
        }
        
        [HttpPost("removeShare")]
        public async Task<IActionResult> RemoveShare(PlaylistShareDTO playlist)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(playlist.PlaylistID);
            var foundUser = await _usersService.GetUserById(playlist.UserID);

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                if (foundUser == null)
                {
                    return BadRequest(new { message = "O id do utilizador que introduziu não existe." });
                }
                else
                {
                    try
                    {
                        await _playListsService.DeleteShare(playlist.PlaylistID, new ObjectId(playlist.UserID));
                        return Ok("O utilizador escolhido foi removido.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return NotFound();
                    }

                }
            }
        }
        
        [HttpPost("removeMultimediaContent")]
        public async Task<IActionResult> DeleteMultimediaContent(DeletePlaylistContentDTO multimediaContent)
        {
            var foundPlaylist = await _playListsService.GetPlaylistById(multimediaContent.PlaylistID);



            //var foundMultimediaContent = await _multimediaContentsService.GetMultimediContentById(multimediaContent.MultimediaContentID);

            if (foundPlaylist == null)
            {
                return BadRequest(new { message = "O id da playlist que introduziu não existe." });
            }
            else
            {
                //if (foundMultimediaContent == null)
                //{
                //    return BadRequest(new { message = "O multimediaContent que introduziu não existe." });
                //}
                //else
                //{
                try
                {
                    await _playListsService.DeleteMultimediaContentInPlaylist(multimediaContent.PlaylistID, new ObjectId(multimediaContent.MultimediaContentID));
                    return Ok("O multimediaContent escolhido foi removido.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return NotFound();
                }
                //}
            }
        }


    }
}
