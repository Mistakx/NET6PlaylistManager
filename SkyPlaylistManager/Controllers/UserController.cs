using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;

namespace SkyPlaylistManager.Controllers;

    [ApiController]
    [Route("[controller]")] // "[controller]" will define the route as /Mongo
    public class MongoController : ControllerBase
    {
        private readonly UsersService _usersService;

       


        public MongoController(UsersService usersService)
        {
            _usersService = usersService;
        }



        [HttpGet]
        public async Task<List<User>> Get() =>
        await _usersService.GetAsync();


        [HttpPost("register")]
        public async Task<IActionResult> Register(User newUser)
        {
        
            var foundUser = await _usersService.GetAsyncByEmail(newUser.Email);

       
            if (foundUser == null)
            {
                var user = new User();
                user.Email = newUser.Email;
                user.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                user.Name = newUser.Name;
                user.ProfilePhotoPath = "Path to default user profile photo";

                try
                {
                    await _usersService.CreateAsync(user);
                    return Ok();
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


    //[HttpPost("login")]
    //public async Task<IActionResult> Login(User newUser)
    //{
    //    List<User> findedUsers = await _usersService.GetAsyncByEmail(newUser.Email); // MELHOR FORMA DE FAZER ISTO ?

    //    if (findedUsers.FirstOrDefault() == null)
    //    {

    //    }

    //}

}