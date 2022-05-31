namespace SkyPlaylistManager.Models.DTOs.UserRequests;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EditNameDto
{
    public string Id { get; set; }
    public string NewName { get; set; }
    public string SessionToken { get; set; }
}