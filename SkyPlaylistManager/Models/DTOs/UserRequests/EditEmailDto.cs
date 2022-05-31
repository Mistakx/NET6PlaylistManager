namespace SkyPlaylistManager.Models.DTOs.UserRequests;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EditEmailDto
{
    public string Id { get; set; }
    public string NewEmail { get; set; }
}