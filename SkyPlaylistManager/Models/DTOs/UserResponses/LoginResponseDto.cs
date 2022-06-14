using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class LoginResponseDto
{
    public string SessionToken { get; set; }
    public string Username { get; set; }

    public LoginResponseDto(string sessionToken, string username)
    {
        SessionToken = sessionToken;
        Username = username;
    }
    
}