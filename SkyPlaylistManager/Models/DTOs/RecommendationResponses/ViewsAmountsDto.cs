using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class ReturnViewsDto
{
    [BsonElement("totalViewsAmount")] public int? TotalViewsAmount { get; set; }
    [BsonElement("weeklyViewsAmount")] public int? WeeklyViewsAmount { get; set; }
    
    public ReturnViewsDto(int totalViewsAmount, int weeklyViewsAmount)
    {
        TotalViewsAmount = totalViewsAmount;
        WeeklyViewsAmount = weeklyViewsAmount;
    }
}