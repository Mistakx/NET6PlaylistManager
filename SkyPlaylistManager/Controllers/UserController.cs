using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Controllers;

[ApiController]
[Route("[controller]")] // "[controller]" will define the route as /User
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;
    private readonly PlaylistsService _playlistsService;
    private readonly FilesManager _filesManager;
    
    public UserController(UsersService usersService, PlaylistsService playlistsService, FilesManager filesManager)
    {
        _usersService = usersService;
        _filesManager = filesManager;
        _playlistsService = playlistsService;
    }



    [HttpGet("Playlists/{userId:length(24)}")]
    public async Task<List<PlaylistBasicDetailsDTO>?> UserPlaylists(string userId)
    {
        var userPlaylists = await _playlistsService.GetPlaylistsByOwner(userId);
        var deserializedPlaylists = new List<PlaylistBasicDetailsDTO>();

        try
        {
            foreach (var playlist in userPlaylists)
            {
                var deserializedPlaylist = BsonSerializer.Deserialize<PlaylistBasicDetailsDTO>(playlist);
                deserializedPlaylists.Add(deserializedPlaylist);

            }
            return deserializedPlaylists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }



    [HttpGet("Profile/{userId:length(24)}")]
    public async Task<UserBasicDetailsDto?> UserProfile(string userId)
    {
        var userProfile = await _usersService.GetUserBasicDetails(userId);
       
        try
        {
            var deserializedUserProfile = BsonSerializer.Deserialize<UserBasicDetailsDto>(userProfile);
            return deserializedUserProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }



    [HttpGet("{userId:length(24)}")]
    public async Task<UserPlaylistsDto?> UserCompleteProfile (string userId)
    {
        var userCompleteProfile = await _usersService.GetUserDetailsAndPlaylists(userId);

        try
        {
            var deserializedUserCompleteProfile = BsonSerializer.Deserialize<UserPlaylistsDto>(userCompleteProfile);
            return deserializedUserCompleteProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }




    [HttpPost]
    [Route("editProfilePhoto")]
    public async Task<IActionResult> EditProfilePhoto(IFormFile file)
    {
       
        FileInfo newPhotoFileInfo = new FileInfo(file.FileName);

       if (_filesManager.IsValidImage(file))
       {
           var generatedFileName = _filesManager.InsertInDiretory(file);
           _filesManager.DeleteFromDiretory();
           await _usersService.UpdateUserProfilePhoto("6261707eff67ad3d4f51d38b", generatedFileName); // TODO: Mudar para o Id da sessão
           
           return Ok("Imagem atualizada.");
       }
       else
       {
           return BadRequest("Formato de imagem inválido.");
       }
    }


    





    //[HttpGet]
    //public async Task<List<UserCollection>> Get() => 
    //    await _usersService.GetAllUsers();


    // MAPPER A FUNCIONA CORRETAMENTE
    //[HttpGet]
    //public async Task<List<UserGetDTO>> Get()
    //{
    //    var result = await _usersService.GetAllUsers();

    //    var config = new MapperConfiguration(cfg =>
    //        cfg.CreateMap<UserCollection, UserGetDTO>()
    //    );

    //     var mapper = new Mapper(config);

    //     return mapper.Map<List<UserGetDTO>>(result);

    //}


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {

        var foundUser = await _usersService.GetUserByEmail(login.Email);

        if (foundUser == null)
        {
            return BadRequest(new { message = "O email que introduziu não existe." });
        }

        else
        {
            if (!BCrypt.Net.BCrypt.Verify(login.Password, foundUser.Password))
            {
                return BadRequest(new { message = "A password que introduziu não é válida." });
            }
            
            HttpContext.Session.SetString("Session_user", foundUser.Id);
            var session = HttpContext.Session.GetString("Session_user");
            Console.WriteLine(session);

            return Ok(session);
        }
    }


    //[HttpPost("teste")]
    //public IActionResult Teste()
    //{
    //    var session = HttpContext.Session.GetString("Session_user");
    //    Console.WriteLine(session);

    //    return Ok(session);

    //}



    [HttpPost("register")]
    public async Task<IActionResult> Register(NewUserDto newUser)
    {

        var foundUser = await _usersService.GetUserByEmail(newUser.Email);


        if (foundUser == null)
        {
            var user = new UserCollection(newUser);
            
            try
            {
                await _usersService.CreateUser(user);
                return Ok("Utilizador registado com sucesso.");
                //return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Ocorreu um erro no registo.");
            }


        }
        else return BadRequest("Já existe um utilizador com este email.");


    }




}