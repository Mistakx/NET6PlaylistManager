using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.CommunityRequests;
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
        private readonly UserRecommendationsService _userRecommendationsService;
        private readonly PlaylistsService _playlistsService;


        public CommunityController(
            CommunityService communityService,
            SessionTokensService sessionTokensService,
            UsersService usersService,
            UserRecommendationsService userRecommendationsService,
            PlaylistsService playlistsService)
        {
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
            _usersService = usersService;
            _userRecommendationsService = userRecommendationsService;
            _playlistsService = playlistsService;
        }

        [HttpPost("togglePlaylistFollow")]
        public async Task<IActionResult> TogglePlaylistFollow(TogglePlaylistFollowDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);

                var requestingPlaylist = await _playlistsService.GetPlaylistById(request.PlaylistId);
                if (requestingPlaylist == null) return BadRequest("Playlist not found");

                if (await _communityService.PlaylistAlreadyBeingFollowedByUser(request.PlaylistId, requestingUserId))
                {
                    await _communityService.UnfollowPlaylist(requestingPlaylist.Id, new ObjectId(requestingUserId),
                        requestingPlaylist.UsersFollowingAmount);
                    return Ok("Successfully unfollowed playlist");
                }

                if (!await _communityService.PlaylistAlreadyBeingFollowedByUser(request.PlaylistId, requestingUserId))
                {
                    await _communityService.FollowPlaylist(requestingPlaylist.Id, new ObjectId(requestingUserId),
                        requestingPlaylist.UsersFollowingAmount);
                    return Ok("Successfully followed playlist");
                }

                return BadRequest("Something went wrong while detecting if user already follows playlist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Unsuccessfully tried to follow/unfollow playlist");
            }
        }
    }
}