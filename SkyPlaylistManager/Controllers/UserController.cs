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

    public UserController(
        UsersService usersService,
        PlaylistsService playlistsService,
        FilesManager filesManager,
        SessionTokensService sessionTokensService
    )
    {
        _usersService = usersService;
        _filesManager = filesManager;
        _playlistsService = playlistsService;
        _sessionTokensService = sessionTokensService;
    }

    [HttpGet("Playlists/{userId:length(24)}")]
    public async Task<List<PlaylistInformationDto>?> UserPlaylists(string userId)
    {
        var userPlaylists = await _playlistsService.GetPlaylistsByOwner(userId);
        var deserializedPlaylists = new List<PlaylistInformationDto>();

        try
        {
            foreach (var playlist in userPlaylists)
            {
                var deserializedPlaylist = BsonSerializer.Deserialize<PlaylistInformationDto>(playlist);
                deserializedPlaylists.Add(deserializedPlaylist);
            }

            return deserializedPlaylists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    [HttpGet("Profile/{sessionToken:length(24)}")]
    public async Task<UserBasicProfileDto?> UserProfile(string sessionToken)
    {
        var userProfile = await _usersService.GetUserBasicDetails(_sessionTokensService.GetUserId(sessionToken));

        try
        {
            var deserializedUserProfile = BsonSerializer.Deserialize<UserBasicProfileDto>(userProfile);
            return deserializedUserProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    [HttpGet("{sessionToken:length(24)}")]
    public async Task<UserBasicProfileDto?> UserCompleteProfile(string sessionToken)
    {
        var userCompleteProfile =
            await _usersService.GetUserDetailsAndPlaylists(_sessionTokensService.GetUserId(sessionToken));

        try
        {
            var deserializedUserCompleteProfile =
                BsonSerializer.Deserialize<UserBasicProfileDto>(userCompleteProfile);
            return deserializedUserCompleteProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    [HttpPost("editProfilePhoto")]
    public async Task<IActionResult> EditProfilePhoto([FromForm] EditProfilePhotoDto request)
    {
        if (!_filesManager.IsValidImage(request.UserPhoto!)) return BadRequest("Invalid image format");
        try
        {
            var userId = _sessionTokensService.GetUserId(request.SessionToken!);
            var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto!, "UsersProfilePhotos");

            var oldPhoto = await _usersService.GetUserProfilePhoto(userId);

            await _usersService.UpdateUserProfilePhoto(userId, "GetImage/UsersProfilePhotos/" + generatedFileName);
            _filesManager.DeleteFromDirectory((string) oldPhoto["profilePhotoUrl"], "UsersProfilePhotos");
            return Ok("Profile photo updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return BadRequest("Error occured while changing profile picture");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
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

        return Ok(session);
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
                user = new UserDocument(request,
                    "https://uploads-ssl.webflow.com/5ff35d7f43faaaadc00d1741/61291e65ed6d7332e7e709dc_depositphotos_137014128-stock-illustration-user-profile-icon.jpeg");
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
        var userId = _sessionTokensService.GetUserId(request.SessionToken!);

        try
        {
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
        var userId = _sessionTokensService.GetUserId(request.SessionToken!);

        var userCurrentInformation = await _usersService.GetUserById(userId);

        if (request.NewEmail == userCurrentInformation?.Email &&
            request.NewUsername == userCurrentInformation?.Username &&
            request.NewName == userCurrentInformation?.Name)
        {
            return BadRequest("No edits were made");
        }

        if (request.NewUsername != userCurrentInformation?.Username)
        {
            var foundUserByUsername = await _usersService.GetUserByUsername(request.NewUsername!);
            if (foundUserByUsername != null) return BadRequest("Username already exists");
        }

        if (request.NewEmail != userCurrentInformation?.Email)
        {
            var foundUserByEmail = await _usersService.GetUserByEmail(request.NewEmail!);
            if (foundUserByEmail != null) return BadRequest("Email already exists");
        }

        await _usersService.UpdateEmail(userId, request.NewEmail!);
        await _usersService.UpdateName(userId, request.NewName!);
        await _usersService.UpdateUsername(userId, request.NewUsername!);
        return Ok("User information successfully updated");
    }

    [HttpPost("editEmail")]
    public async Task<IActionResult> EditEmail(EditEmailDto email)
    {
        var foundUser = await _usersService.GetUserById(email.Id!);

        if (foundUser == null) return BadRequest("Email doesn't exist");

        await _usersService.UpdateEmail(email.Id!, email.NewEmail);
        return Ok("Email successfully updated");
    }

    [HttpPost("editName")]
    public async Task<IActionResult> EditName(EditNameDto name)
    {
        var foundUser = await _usersService.GetUserById(name.Id!);

        if (foundUser == null) return BadRequest(new {message = "ID doesn't exist"});

        await _usersService.UpdateName(name.Id!, name.NewName!);
        return Ok("Name successfully updated");
    }
}