using AutoMapper;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;

namespace SkyPlaylistManager.Controllers;

[ApiController]
[Route("[controller]")] // "[controller]" will define the route as /User
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;


    public UserController(UsersService usersService)
    {
        _usersService = usersService;
    }

    

    [HttpGet("{userId:length(24)}")]
    public async Task<List<UserPlaylistsDto>> UserPlaylists(string userId)
    {
        var userPlaylists = await _usersService.GetUserPlaylists(userId);
        var deserializedUserPlaylists = new List<UserPlaylistsDto>();

        try
        {
            foreach (var user in userPlaylists)
            {
                var model = BsonSerializer.Deserialize<UserPlaylistsDto>(user);
                deserializedUserPlaylists.Add(model);
            }

            return deserializedUserPlaylists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
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
    public async Task<IActionResult> Login(LoginDTO login)
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



    [HttpPost("register")]
    public async Task<IActionResult> Register(NewUserDTO newUser)
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