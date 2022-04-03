using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models;

namespace SkyPlaylistManager.Controllers;

    [ApiController]
    [Route("[controller]")]
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




}