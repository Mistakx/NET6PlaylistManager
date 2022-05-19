using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.UserRequests;

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


    [HttpGet("GetImage/{imageName}")] // https://stackoverflow.com/questions/186062/can-an-asp-net-mvc-controller-return-an-image
    public Task<IActionResult> GetImage(string imageName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
        return Task.FromResult<IActionResult>(PhysicalFile(path, "image/jpeg"));
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


    [HttpGet("{username}")]
    public async Task<List<UserCompleteProfileDto>?> UsernamePlaylists(string username)
    {
        var usernamePlaylists = await _usersService.GetUserNamePlaylists(username);
        var deserializedUsernamePlaylists = new List<UserCompleteProfileDto>();

        try
        {
            foreach (var user in usernamePlaylists)
            {
                var model = BsonSerializer.Deserialize<UserCompleteProfileDto>(user);
                deserializedUsernamePlaylists.Add(model);
            }

            return deserializedUsernamePlaylists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
    public async Task<UserCompleteProfileDto?> UserCompleteProfile(string sessionToken)
    {
        var userCompleteProfile = await _usersService.GetUserDetailsAndPlaylists(_sessionTokensService.GetUserId(sessionToken));

        try
        {
            var deserializedUserCompleteProfile = BsonSerializer.Deserialize<UserCompleteProfileDto>(userCompleteProfile);
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
    public async Task<IActionResult> EditProfilePhoto([FromForm] EditProfilePhotoDto request)
    {
        if (!_filesManager.IsValidImage(request.UserPhoto!)) return BadRequest("Invalid image format.");
        try
        {
            var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto!);
            _filesManager.DeleteFromDirectory(request.SessionToken!);
            await _usersService.UpdateUserProfilePhoto(request.SessionToken!, "User/GetImage/" + generatedFileName);
            return Ok("Profile photo updated successfully.");
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return BadRequest("Error occured while changing profile picture.");
        }


    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var foundUser = await _usersService.GetUserByEmail(login.Email);

        if (foundUser == null) return BadRequest(new {message = "Email doesn't exist."});

        if (!BCrypt.Net.BCrypt.Verify(login.Password, foundUser.Password))
        {
            return BadRequest(new {message = "Invalid password."});
        }

        HttpContext.Session.SetString("Session_user", foundUser.Id!);
        var session = HttpContext.Session.GetString("Session_user");
        Console.WriteLine(session);

        return Ok(session);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] UserSignupDto request)
    {
        var foundUser = await _usersService.GetUserByEmail(request.Email);

        if (foundUser != null) return BadRequest("Email already taken.");

        if (!_filesManager.IsValidImage(request.UserPhoto)) return BadRequest("Invalid image used on user register.");

        try
        {
            var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto);
            var user = new UserCollection(request, "User/GetImage/" + generatedFileName);

            await _usersService.CreateUser(user);
            return Ok("User successfully registered.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error occurred on user register.");
        }
    }


    [HttpPost("editPassword")]
    public async Task<IActionResult> EditPassword(EditPasswordDto password)
    {
        var foundUser = await _usersService.GetUserById(password.Id!);

        if (foundUser == null) return BadRequest(new {message = "ID doesn't exist."});

        if (!BCrypt.Net.BCrypt.Verify(password.OldPassword, foundUser.Password))
        {
            return BadRequest(new {message = "Invalid current password."});
        }

        var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(password.NewPassword);
        await _usersService.UpdatePassword(password.Id!, encryptedPassword);
        return Ok("Password successfully updated.");
    }

    [HttpPost("editEmail")]
    public async Task<IActionResult> EditEmail(EditEmailDto email)
    {
        var foundUser = await _usersService.GetUserById(email.Id!);

        if (foundUser == null) return BadRequest(new {message = "Email doesn't exist."});

        await _usersService.UpdateEmail(email.Id!, email.NewEmail);
        return Ok("Email successfully updated.");
    }


    [HttpPost("editName")]
    public async Task<IActionResult> EditName(EditNameDto name)
    {
        var foundUser = await _usersService.GetUserById(name.Id!);

        if (foundUser == null) return BadRequest(new {message = "ID doesn't exist."});

        await _usersService.UpdateName(name.Id!, name.NewName!);
        return Ok("Name successfully updated.");
    }
}