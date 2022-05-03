using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;

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

       


        [HttpPost("addToPlaylist")]
        public async Task<IActionResult> AddMultimediaContentToPlaylist(JsonObject request)
        {
             MultimediaContent multimediaContent;
           
            try
            {
                string type = (string)request["platform"];
                _multimediaContentFactory._args = request;
                multimediaContent = _multimediaContentFactory[type];
                
                

                await _multimediaContentsService.CreateMultimediaContent(multimediaContent);
                return Ok(multimediaContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }

        }




        [HttpPost("share")]
        public async Task<IActionResult> SharePlaylist(PlaylistShare newPlaylistShare)
        {
            try
            {
                await _playListsService.InsertUserInSharedWithArray(newPlaylistShare.PlaylistID, new ObjectId(newPlaylistShare.UserID));
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
