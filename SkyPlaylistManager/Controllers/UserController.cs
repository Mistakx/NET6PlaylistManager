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
            var user = new User();

            user.UserName = newUser.UserName;
            BCrypt.Net.BCrypt.GenerateSalt();
            user.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);


            
            //return Ok();
            try
            {
                await _usersService.CreateAsync(user);
                return Ok();
                //return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception ex) {
                Console.WriteLine(ex); 
                return BadRequest();
            }

             
        }


        //[HttpPost("login")]
        //public async Task<IActionResult> Login(User newUser)
        //{

        //}

    }