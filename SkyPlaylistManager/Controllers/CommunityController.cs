using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.CommunityRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.UserResponses;
using SkyPlaylistManager.Services;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunityController : Controller
    {
        private readonly CommunityService _communityService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly UsersService _usersService;


        public CommunityController(
            CommunityService communityService,
            SessionTokensService sessionTokensService,
            UsersService usersService
        )
        {
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
            _usersService = usersService;
        }

        [HttpPost("getUsers")]
        public async Task<List<UserBasicProfileDto>?> GetUsers(GetUsersDto request)
        {
            try
            {
                var userId = _sessionTokensService.GetUserId(request.SessionToken);
                var requestingUser = await _usersService.GetUserById(userId);

                var users = await _communityService.GetUsers(request.Username);

                var deserializedUsers = new List<UserBasicProfileDto>();
                foreach (var user in users)
                {
                    if (user.Username != requestingUser?.Username)
                    {
                        var deserializedUser = new UserBasicProfileDto(user.Name, user.Username, user.ProfilePhotoUrl);
                        deserializedUsers.Add(deserializedUser);
                    }
                }

                return deserializedUsers;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}