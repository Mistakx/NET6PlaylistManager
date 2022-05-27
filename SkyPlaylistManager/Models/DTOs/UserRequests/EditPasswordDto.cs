namespace SkyPlaylistManager.Models.DTOs.UserRequests;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EditPasswordDto
{
    public string? OldPassword { get; set; }

    public string? NewPassword { get; set; }
    
    public string? SessionToken { get; set; }
}