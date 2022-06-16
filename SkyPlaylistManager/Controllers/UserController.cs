using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.UserRequests;
using SkyPlaylistManager.Models.DTOs.UserResponses;
using LoginDto = SkyPlaylistManager.Models.DTOs.UserRequests.LoginDto;

namespace SkyPlaylistManager.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;
    private readonly PlaylistsService _playlistsService;
    private readonly FilesManager _filesManager;
    private readonly SessionTokensService _sessionTokensService;
    private readonly UserRecommendationsService _userRecommendationsService;
    private readonly PlaylistRecommendationsService _playlistRecommendationsService;


    public UserController(
        UsersService usersService,
        PlaylistsService playlistsService,
        FilesManager filesManager,
        SessionTokensService sessionTokensService,
        UserRecommendationsService userRecommendationsService,
        PlaylistRecommendationsService playlistRecommendationsService)
    {
        _usersService = usersService;
        _filesManager = filesManager;
        _playlistsService = playlistsService;
        _sessionTokensService = sessionTokensService;
        _userRecommendationsService = userRecommendationsService;
        _playlistRecommendationsService = playlistRecommendationsService;
    }

    [HttpPost("Playlists/")]
    public async Task<List<PlaylistDto>?> GetUserPlaylists(GetUserPlaylistsDto request)
    {
        try
        {
            var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
            var requestingUser = await _usersService.GetUserById(requestingUserId);
            var requestedUser = await _usersService.GetUserByUsername(request.Username);
            if (requestingUser == null || requestedUser == null) return null;
            
            var requestedUserPlaylists = await _usersService.GetUserPlaylists(requestedUser.Id);
            var requestedUserDeserializedPlaylists = BsonSerializer.Deserialize<GetUserPlaylistsLookupDto>(requestedUserPlaylists);
            
            // The playlists need to be returned in the order set by the user, which is not the order when you do a lookup.
            // It is the order of the ids in the userDocument playlistIds field.
            var orderedPlaylists = new List<PlaylistDto>();
            foreach (var playlistContentId in requestedUser.UserPlaylistIds) // The playlists ids are in the order sorted by the user
            {
                foreach (var playlistDocument in requestedUserDeserializedPlaylists.Playlists)
                {
                    if (playlistDocument.Id == playlistContentId.ToString())
                    {
                        if (playlistDocument.Visibility == "Public")
                        {
                            var currentPlaylistViews =
                                await _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(playlistDocument
                                    .Id);

                            int weeklyViews;
                            int totalViews;
                            if (currentPlaylistViews != null)
                            {
                                weeklyViews = currentPlaylistViews.WeeklyViewsAmount;
                                totalViews = currentPlaylistViews.TotalViewsAmount;
                            }
                            else
                            {
                                weeklyViews = 0;
                                totalViews = 0;
                            }

                            orderedPlaylists.Add(new PlaylistDto(playlistDocument.Id, playlistDocument.Title, playlistDocument.Description, playlistDocument.ThumbnailUrl,
                                playlistDocument.ResultsAmount, weeklyViews, totalViews));     
                        }

                        if (playlistDocument.Visibility == "Private")
                        {
                            orderedPlaylists.Add(new PlaylistDto(playlistDocument.Id, playlistDocument.Title, playlistDocument.Description, playlistDocument.ThumbnailUrl,
                                playlistDocument.ResultsAmount));     
                        }
 
                        

                    }
                }
            }

            if (requestingUser.Username == requestedUser.Username)
            {
                return orderedPlaylists;
            }

            if (requestingUser.Username != requestedUser.Username)
            {
                var publicPlaylists = new List<PlaylistDto>();
                foreach (var playlist in orderedPlaylists)
                {
                    if (playlist.Visibility == "Public")
                    {
                        publicPlaylists.Add(playlist);
                    }
                }

                return publicPlaylists;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    [HttpPost("Profile/")]
    public async Task<UserProfileDto?> GetUserProfile(GetUserProfileDto request)
    {
        try
        {
            var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
            var requestingUser = await _usersService.GetUserById(requestingUserId);
            var requestedUser = await _usersService.GetUserByUsername(request.Username);
            var requestedUserRecommendations =
                await _userRecommendationsService.GetUserRecommendationsDocumentById(requestedUser?.Id!);

            if (requestingUser == null || requestedUser == null) return null;

            int requestedUserWeeklyViews;
            int requestedUserTotalViews;
            if (requestedUserRecommendations != null)
            {
                requestedUserWeeklyViews = requestedUserRecommendations.WeeklyViewsAmount;
                requestedUserTotalViews = requestedUserRecommendations.TotalViewsAmount;
            }
            else
            {
                requestedUserWeeklyViews = 0;
                requestedUserTotalViews = 0;
            }

            UserProfileDto response;
            if (requestedUser.Username == requestingUser.Username)
            {
                response = new UserProfileDto(requestedUser.Email,
                    requestedUser.Name, requestedUser.Username,
                    requestedUser.ProfilePhotoUrl, requestedUserWeeklyViews,
                    requestedUserTotalViews);
                return response;
            }

            response = new UserProfileDto(
                requestedUser.Name, requestedUser.Username, requestedUser.ProfilePhotoUrl,
                requestedUserWeeklyViews, requestedUserTotalViews);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    [HttpPost("editProfilePhoto")]
    public async Task<IActionResult> EditProfilePhoto([FromForm] EditProfilePhotoDto request)
    {
        if (!_filesManager.IsValidImage(request.UserPhoto!)) return BadRequest("Invalid image format");
        try
        {
            var userId = _sessionTokensService.GetUserIdFromToken(request.SessionToken!);
            var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto!, "UsersProfilePhotos");

            var currentPhotoUrl = (await _usersService.GetUserById(userId))?.ProfilePhotoUrl;

            await _usersService.UpdateUserProfilePhoto(userId, "GetImage/UsersProfilePhotos/" + generatedFileName);

            if (currentPhotoUrl != null && !currentPhotoUrl.Contains("/GetImage/UserProfilePhotos/DefaultUserPhoto"))
            {
                _filesManager.DeleteFromDirectory(currentPhotoUrl!, "UsersProfilePhotos");
            }

            return Ok("Profile photo updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return BadRequest("Error occured while changing profile picture");
        }
    }

    [HttpPost("login")]
    public async Task<dynamic> Login(LoginDto login)
    {
        try
        {
            var foundUser = await _usersService.GetUserByEmail(login.Email);

            if (foundUser == null) return BadRequest("Email doesn't exist");

            if (!BCrypt.Net.BCrypt.Verify(login.Password, foundUser.Password))
            {
                return BadRequest("Invalid password");
            }

            HttpContext.Session.SetString("Session_user", foundUser.Id!);
            var session = HttpContext.Session.GetString("Session_user");
            Console.WriteLine(session);

            return new LoginResponseDto(session!, foundUser.Username);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return BadRequest("Error occured while logging in");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] UserSignupDto request)
    {
        try
        {
            var foundUserByEmail = await _usersService.GetUserByEmail(request.Email);

            if (foundUserByEmail != null) return BadRequest("Email already taken");

            var foundUserByUsername = await _usersService.GetUserByUsername(request.Username);

            if (foundUserByUsername != null) return BadRequest("Username already taken");

            UserDocument user;
            if (request.UserPhoto == null)
            {
                user = new UserDocument(request, "GetImage/UsersProfilePhotos/DefaultUserPhoto.jpeg");
            }
            else
            {
                var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto, "UsersProfilePhotos");
                user = new UserDocument(request, "GetImage/UsersProfilePhotos/" + generatedFileName);
                if (!_filesManager.IsValidImage(request.UserPhoto))
                    return BadRequest("Invalid image used on user register");
            }

            await _usersService.CreateUser(user);
            return Ok("User successfully registered");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error occurred on user register");
        }
    }

    [HttpPost("editPassword")]
    public async Task<IActionResult> EditPassword(EditPasswordDto request)
    {
        try
        {
            var userId = _sessionTokensService.GetUserIdFromToken(request.SessionToken!);

            if (request.CurrentPassword == request.NewPassword)
                return BadRequest("New password must be different from current password");

            var foundUser = await _usersService.GetUserById(userId);

            if (foundUser == null) return BadRequest("User ID doesn't exist");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, foundUser.Password))
            {
                return BadRequest("Invalid current password");
            }

            var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _usersService.UpdatePassword(userId, encryptedPassword);
            return Ok("Password successfully updated.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Password  occurred on password update");
        }
    }

    [HttpPost("editUserInfo")]
    public async Task<IActionResult> EditUserInfo(EditUserInfoDto request)
    {
        try
        {
            var userId = _sessionTokensService.GetUserIdFromToken(request.SessionToken!);

            var userCurrentInformation = await _usersService.GetUserById(userId);

            if (request.NewEmail == userCurrentInformation?.Email &&
                request.NewUsername == userCurrentInformation.Username &&
                request.NewName == userCurrentInformation.Name)
            {
                return BadRequest("No edits were made");
            }

            if (request.NewUsername != userCurrentInformation?.Username)
            {
                var foundUserByUsername = await _usersService.GetUserByUsername(request.NewUsername);
                if (foundUserByUsername != null) return BadRequest("Username already exists");
            }

            if (request.NewEmail != userCurrentInformation?.Email)
            {
                var foundUserByEmail = await _usersService.GetUserByEmail(request.NewEmail);
                if (foundUserByEmail != null) return BadRequest("Email already exists");
            }

            await _usersService.UpdateEmail(userId, request.NewEmail);
            await _usersService.UpdateName(userId, request.NewName);
            await _usersService.UpdateUsername(userId, request.NewUsername);
            return Ok("User information successfully updated");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error occurred on user information update");
        }
    }

    [HttpPost("editEmail")]
    public async Task<IActionResult> EditEmail(EditEmailDto email)
    {
        try
        {
            var foundUser = await _usersService.GetUserById(email.Id!);

            if (foundUser == null) return BadRequest("Email doesn't exist");

            await _usersService.UpdateEmail(email.Id, email.NewEmail);
            return Ok("Email successfully updated");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error occurred on email update");
        }
    }

    [HttpPost("editName")]
    public async Task<IActionResult> EditName(EditNameDto name)
    {
        try
        {
            var foundUser = await _usersService.GetUserById(name.Id);

            if (foundUser == null) return BadRequest(new {message = "ID doesn't exist"});

            await _usersService.UpdateName(name.Id, name.NewName!);
            return Ok("Name successfully updated");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error occurred on name update");
        }
    }
}