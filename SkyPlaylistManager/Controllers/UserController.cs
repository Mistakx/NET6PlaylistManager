using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs;
using SkyPlaylistManager.Models.DTOs.Playlist;
using SkyPlaylistManager.Models.DTOs.User;

namespace SkyPlaylistManager.Controllers;

[ApiController]
[Route("[controller]")] // "[controller]" will define the route as /User
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;
    private readonly PlaylistsService _playlistsService;
    private readonly FilesManager _filesManager;

    public UserController(UsersService usersService, PlaylistsService playlistsService, FilesManager filesManager)
    {
        _usersService = usersService;
        _filesManager = filesManager;
        _playlistsService = playlistsService;
    }


    [HttpGet("GetImage/{imageName}")] // https://stackoverflow.com/questions/186062/can-an-asp-net-mvc-controller-return-an-image
    public async Task<IActionResult> GetImage(string imageName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
        return PhysicalFile(path, "image/jpeg");
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
    public async Task<List<UserProfileDto>> UsernamePlaylists(string username)
    {
        var usernamePlaylists = await _usersService.GetUserNamePlaylists(username);
        var deserializedUsernamePlaylists = new List<UserProfileDto>();

        try
        {
            foreach (var user in usernamePlaylists)
            {
                var model = BsonSerializer.Deserialize<UserProfileDto>(user);
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


    [HttpGet("Profile/{userId:length(24)}")]
    public async Task<UserBasicDetailsDto?> UserProfile(string userId)
    {
        var userProfile = await _usersService.GetUserBasicDetails(userId);

        try
        {
            var deserializedUserProfile = BsonSerializer.Deserialize<UserBasicDetailsDto>(userProfile);
            return deserializedUserProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }


    [HttpGet("{userId:length(24)}")]
    public async Task<UserProfileDto?> UserCompleteProfile(string userId)
    {
        var userCompleteProfile = await _usersService.GetUserDetailsAndPlaylists(userId);

        try
        {
            var deserializedUserCompleteProfile = BsonSerializer.Deserialize<UserProfileDto>(userCompleteProfile);
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
        FileInfo newPhotoFileInfo = new FileInfo(request.UserPhoto!.FileName);

        if (_filesManager.IsValidImage(request.UserPhoto))
        {
            var generatedFileName = _filesManager.InsertInDirectory(request.UserPhoto);
            _filesManager.DeleteFromDirectory(request.SessionToken!);
            await _usersService.UpdateUserProfilePhoto(request.SessionToken!, "User/GetImage/" + generatedFileName);

            return Ok("User/GetImage/" + generatedFileName);
        }
        else
        {
            return BadRequest("Formato de imagem inválido.");
        }
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var foundUser = await _usersService.GetUserByEmail(login.Email);

        if (foundUser == null)
        {
            return BadRequest(new {message = "O email que introduziu não existe."});
        }

        else
        {
            if (!BCrypt.Net.BCrypt.Verify(login.Password, foundUser.Password))
            {
                return BadRequest(new {message = "A password que introduziu não é válida."});
            }

            HttpContext.Session.SetString("Session_user", foundUser.Id!);
            var session = HttpContext.Session.GetString("Session_user");
            Console.WriteLine(session);

            return Ok(session);
        }
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
            return BadRequest("Error on user register.");
        }
    }


    [HttpPost("editPassword")]
    public async Task<IActionResult> EditPassword(EditPasswordDTO password)
    {
        var foundUser = await _usersService.GetUserById(password.Id);

        if (foundUser == null)
        {
            return BadRequest(new {message = "O ID que introduziu não existe."});
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify(password.OldPassword, foundUser.Password))
            {
                return BadRequest(new {message = "A password atual é inválida."});
            }
            else
            {
                var EncryptedPassword = BCrypt.Net.BCrypt.HashPassword(password.NewPassword);
                await _usersService.UpdatePassword(password.Id, EncryptedPassword);
                return Ok("A password foi atualizada com sucesso.");
            }
        }
    }


    [HttpPost("editEmail")]
    public async Task<IActionResult> EditEmail(EditEmailDTO email)
    {
        var foundUser = await _usersService.GetUserById(email.Id);

        if (foundUser == null)
        {
            return BadRequest(new {message = "O email que introduziu não existe."});
        }
        else
        {
            await _usersService.UpdateEmail(email.Id, email.NewEmail);
            return Ok("O email foi atualizado com sucesso.");
        }
    }


    [HttpPost("editName")]
    public async Task<IActionResult> EditName(EditNameDTO name)
    {
        var foundUser = await _usersService.GetUserById(name.Id);

        if (foundUser == null)
        {
            return BadRequest(new {message = "O id que introduziu não existe."});
        }
        else
        {
            await _usersService.UpdateName(name.Id, name.NewName);

            return Ok("O nome foi atualizado com sucesso.");
        }
    }
}