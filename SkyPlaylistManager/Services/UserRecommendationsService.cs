using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;

namespace SkyPlaylistManager.Services
{
    public class UserRecommendationsService
    {
        private readonly IMongoCollection<UserRecommendationsDocument> _recommendationsCollection;
        private readonly string _userCollectionName;

        public UserRecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<UserRecommendationsDocument>(databaseSettings.Value
                    .UserRecommendationsCollectionName);

            _userCollectionName = databaseSettings.Value.UsersCollectionName;
        }

        private async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter = Builders<UserRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "viewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<UserRecommendationsDocument>.Update.Pull("viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateOneAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }

        public async void UpdateRecommendationsWeeklyViews()
        {
            var recommendationsWithOldDatesFilter = Builders<UserRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "viewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var recommendationWithOldDates =
                await _recommendationsCollection.Find(recommendationsWithOldDatesFilter).ToListAsync();

            DeleteOldRecommendations();

            foreach (var recommendation in recommendationWithOldDates)
            {
                var recommendationWithNewDates =
                    await _recommendationsCollection.Find(
                            Builders<UserRecommendationsDocument>.Filter.Eq("_id", new ObjectId(recommendation.Id)))
                        .FirstOrDefaultAsync();

                await _recommendationsCollection.UpdateOneAsync(p => p.Id == recommendation.Id,
                    Builders<UserRecommendationsDocument>.Update.Set("viewsAmount",
                        recommendationWithNewDates.WeeklyViewDates.Count));
            }
        }

        public async Task<List<GetTrendingUsersLookupDto>?> GetTrendingUsers(string usernameBeginningLetters,
            int resultsLimit)
        {
            var trendingUsers = await _recommendationsCollection.Aggregate()
                .Lookup(_userCollectionName, "userId", "_id", "user")
                .Unwind("user")
                .Match(Builders<BsonDocument>.Filter
                    .Regex("user.username", new BsonRegularExpression("(?i)^" + usernameBeginningLetters)))
                .ToListAsync();

            var deserializedTrendingUsers = new List<GetTrendingUsersLookupDto>();
            foreach (var trendingUser in trendingUsers)
            {
                var deserializedTrendingUser = BsonSerializer.Deserialize<GetTrendingUsersLookupDto>(trendingUser);
                deserializedTrendingUsers.Add(deserializedTrendingUser);
            }

            deserializedTrendingUsers.Sort((x, y) => y.WeeklyViewDates.Count.CompareTo(x.WeeklyViewDates.Count));
            return deserializedTrendingUsers.Take(resultsLimit).ToList();
        }


        public async Task SaveUserView(string userId)
        {
            var recommendationsDocument = new UserRecommendationsDocument(userId);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }

        public async Task AddViewToUser(
            string userId,
            int totalViewAmount
        )
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);


            var weeklyViewsUpdate = Builders<UserRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }

        public async Task<UserRecommendationsDocument?> GetUserRecommendationsDocumentById(string userId)
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);

            var result = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return result;
        }
    }
}