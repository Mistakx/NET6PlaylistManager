using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.ContentResponses;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : Controller
    {
        private const string PlaylistIdDoesntExistMessage = "Playlist ID doesn't exist";
        private readonly FilesManager _filesManager;
        private readonly ContentService _contentService;
        private readonly PlaylistsService _playListsService;
        private readonly SessionTokensService _sessionTokensService;
        private readonly UsersService _usersService;
        private readonly PlaylistRecommendationsService _playlistRecommendationsService;
        private readonly CommunityService _communityService;

        public PlaylistController(
            PlaylistsService playlistsService,
            UsersService usersService,
            ContentService contentService,
            SessionTokensService sessionTokensService,
            FilesManager filesManager,
            PlaylistRecommendationsService playlistRecommendationsService,
            CommunityService communityService)
        {
            _playListsService = playlistsService;
            _usersService = usersService;
            _contentService = contentService;
            _sessionTokensService = sessionTokensService;
            _filesManager = filesManager;
            _playlistRecommendationsService = playlistRecommendationsService;
            _communityService = communityService;
        }

        // CREATE

        [HttpPost("createPlaylist")]
        public async Task<IActionResult> CreatePlaylist(CreatePlaylistDto request)
        {
            try
            {
                var userId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var playlist = new PlaylistDocument(request, _sessionTokensService);
                await _playListsService.CreatePlaylist(playlist, userId);
                return Ok("Playlist successfully created");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while creating playlist");
            }
        }


        // READ

        [HttpPost("getPlaylistInformation")]
        public async Task<PlaylistInformationDto?> GetPlaylistInformation(GetPlaylistInformationDto request)
        {
            try
            {
                var playlistInformationDtoBuilder = new PlaylistInformationDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _usersService.GetUserById(requestingUserId);
                if (requestingUser == null) return null;

                var requestedPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return null;

                // Playlist owned by requesting user
                if (requestedPlaylist.OwnerId == requestingUser.Id)
                {
                    switch (requestedPlaylist.Visibility)
                    {
                        case "Private":
                            return playlistInformationDtoBuilder.BeginBuilding(requestedPlaylist.Id,
                                requestedPlaylist.Title,
                                requestedPlaylist.Description, requestedPlaylist.ThumbnailUrl,
                                requestedPlaylist.ResultIds.Count).AddVisibility("Private").Build();
                        case "Public":
                        {
                            var requestedPlaylistViews = await
                                _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(
                                    request.PlaylistId);

                            return playlistInformationDtoBuilder.BeginBuilding(requestedPlaylist.Id,
                                    requestedPlaylist.Title,
                                    requestedPlaylist.Description, requestedPlaylist.ThumbnailUrl,
                                    requestedPlaylist.ResultIds.Count).AddVisibility("Public")
                                .AddViews(requestedPlaylistViews!)
                                .Build();
                        }
                    }
                }

                // Playlist not owned by requesting user
                else
                {
                    if (requestedPlaylist.Visibility != "Public") return null;

                    var requestedPlaylistViews = await
                        _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(request.PlaylistId);

                    var requestedPlaylistOwner = await
                        _usersService.GetUserById(requestedPlaylist.OwnerId);

                    return playlistInformationDtoBuilder.BeginBuilding(requestedPlaylist.Id,
                            requestedPlaylist.Title,
                            requestedPlaylist.Description, requestedPlaylist.ThumbnailUrl,
                            requestedPlaylist.ResultIds.Count).AddViews(requestedPlaylistViews!)
                        .AddOwner(requestedPlaylistOwner!).Build();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }


        [HttpGet("getPlaylistContent/{playlistId}")] // TODO: Add security
        public async Task<List<UnknownContentResponseDto>?> GetPlaylistContent(string playlistId)
        {
            try
            {
                var unknownContentResponseDtoBuilder = new UnknownContentResponseDtoBuilder();

                var playlistResults = await _playListsService.GetPlaylistContent(playlistId);
                var deserializedPlaylistResults =
                    BsonSerializer.Deserialize<GetPlaylistContentLookupDto>(playlistResults);

                var playlistResultsOrderedIds = await _playListsService.GetPlaylistContentOrderedIds(playlistId);
                var deserializedPlaylistResultsOrderedIds =
                    BsonSerializer.Deserialize<PlaylistContentsOrderedIdsDto>(playlistResultsOrderedIds);

                var orderedPlaylistResults = new List<UnknownContentResponseDto>();
                foreach (var playlistContentId in deserializedPlaylistResultsOrderedIds.ResultIds)
                {
                    foreach (var playlistContent in deserializedPlaylistResults.Content)
                    {
                        if (playlistContent.DatabaseId == playlistContentId.ToString())
                        {
                            orderedPlaylistResults.Add(unknownContentResponseDtoBuilder.BeginBuilding(playlistContent)
                                .Build());
                        }
                    }
                }

                return orderedPlaylistResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }


        // UPDATE

        [HttpPost("sortContent")]
        public async Task<IActionResult> SortResult(SortContentDto request)
        {
            try
            {
                var generalResultId = ObjectId.Parse(request.GeneralizedResultDatabaseId);


                var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);
                await _playListsService.DeleteContentIdFromPlaylist(request.PlaylistId, generalResultId);

                foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);
                await _playListsService.DeleteContentIdFromPlaylist(request.PlaylistId, generalResultId);
                await _playListsService.InsertContentInSpecificPlaylistPosition(request);
                return Ok("Successfully sorted result");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while sorting result");
            }
        }

        [HttpPost("addContent")]
        public async Task<IActionResult> AddContentToPlaylist(AddContentToPlaylistDto request)
        {
            try
            {
                var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                if (await _playListsService.ContentIsAlreadyInPlaylist(request.PlaylistId, request.Content.Title,
                        request.Content.PlayerFactoryName, request.Content.PlatformPlayerUrl!))
                {
                    return BadRequest("Result already in playlist");
                }

                await _contentService.CreateContent(request.Content);
                var generalizedResultId = ObjectId.Parse(request.Content.DatabaseId);

                await _playListsService.InsertContentIdInPlaylist(request.PlaylistId, generalizedResultId);
                return Ok("Successfully added to playlist");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occurred while adding to playlist");
            }
        }

        [HttpPost("editPlaylistPhoto")]
        public async Task<IActionResult> EditProfilePhoto([FromForm] EditPlaylistThumbnail request)
        {
            if (!_filesManager.IsValidImage(request.PlaylistPhoto)) return BadRequest("Invalid image format");
            try
            {
                var requestedPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                var generatedFileName = _filesManager.InsertInDirectory(request.PlaylistPhoto, "PlaylistsThumbnails");

                await _playListsService.UpdatePlaylistPhoto(request.PlaylistId,
                    "GetImage/PlaylistsThumbnails/" + generatedFileName);
                _filesManager.DeleteFromDirectory((string) requestedPlaylist.ThumbnailUrl, "PlaylistsThumbnails");
                return Ok("GetImage/PlaylistsThumbnails/" + generatedFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occured while changing profile picture");
            }
        }

        [HttpPost("setCover")]
        public async Task<IActionResult> SetCoverItem(SetPlaylistCoverDto request)
        {
            try
            {
                var requestedPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);
                if (requestedPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                await _playListsService.UpdatePlaylistPhoto(request.PlaylistId, request.CoverUrl);
                try
                {
                    _filesManager.DeleteFromDirectory(requestedPlaylist.ThumbnailUrl, "PlaylistsThumbnails");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return Ok("Successfully set cover photo");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Error occured while changing cover photo");
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditPlaylist(EditPlaylistDto request)
        {
            try
            {
                await _playListsService.UpdatePlaylistInformation(request);
                return Ok("Playlist successfully edited");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while editing playlist");
            }
        }

        // DELETE 

        [HttpPost("deletePlaylist")] // TODO: Add security
        public async Task<IActionResult> DeletePlaylist(DeletePlaylistDto request)
        {
            try
            {
                var foundPlaylist = await _playListsService.GetPlaylistById(request.Id);


                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                var id = ObjectId.Parse(request.Id);
                await _playListsService.DeletePlaylist(request.Id);

                var owner = foundPlaylist.OwnerId;
                await _usersService.DeletePlaylistIdFromUser(owner, id);

                return Ok("Playlist successfully deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Error while deleting playlist");
            }
        }

        [HttpPost("deleteContent")] // TODO: Add security
        public async Task<IActionResult> DeleteGeneralizedResult(DeletePlaylistContentDto request)
        {
            try
            {
                var foundPlaylist = await _playListsService.GetPlaylistById(request.PlaylistId);

                if (foundPlaylist == null) return BadRequest(PlaylistIdDoesntExistMessage);

                var generalizedResultToDeleteId = new ObjectId(request.GeneralizedResultDatabaseId);
                await _playListsService.DeleteContentIdFromPlaylist(request.PlaylistId, generalizedResultToDeleteId);
                return Ok("Successfully removed from playlist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Error while removing from playlist");
            }
        }
    }
}