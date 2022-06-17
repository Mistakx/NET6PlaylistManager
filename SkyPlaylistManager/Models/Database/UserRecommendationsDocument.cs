using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyPlaylistManager.Models.Database
{
    public class UserRecommendationsDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("weeklyViewDates")]
        public List<DateTime> WeeklyViewDates { get; set; }

        [BsonElement("totalViewsAmount")] public int TotalViewsAmount { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public UserRecommendationsDocument(string userId)
        {
            WeeklyViewDates = new List<DateTime> {DateTime.Now};
            TotalViewsAmount = 1;
            UserId = userId;
        }
    }
}