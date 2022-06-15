using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("title")] public string Title { get; set; }

    [BsonElement("visibility")] public string Visibility { get; set; }
    
    [BsonElement("description")] public string Description { get; set; }

    [BsonElement("thumbnailUrl")] public string ThumbnailUrl { get; set; }
    [BsonElement("resultsAmount")] public int ResultsAmount { get; set; }

    public int WeeklyViewsAmount { get; set; }

    public int TotalViewsAmount { get; set; }

    public PlaylistDto(string playlistId, string title, string visibility, string description,
        string thumbnailUrl, int resultsAmount, int weeklyViewsAmount, int totalViewsAmount)
    {
        Id = playlistId;
        Title = title;
        Visibility = visibility;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        ResultsAmount = resultsAmount;
        WeeklyViewsAmount = weeklyViewsAmount;
        TotalViewsAmount = totalViewsAmount;
    }
}