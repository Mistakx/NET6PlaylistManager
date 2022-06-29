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

        // CREATE

        public async Task SaveUserView(string userId)
        {
            var recommendationsDocument = new UserRecommendationsDocument(userId);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }


        // READ

        public async Task<UserRecommendationsDocument?> GetUserRecommendationsDocumentById(string userId)
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);

            var result = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return result;
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

        
        // UPDATE

        public async Task AddViewToUser(string userId, int totalViewAmount)
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);


            var weeklyViewsUpdate = Builders<UserRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }
        
        
        // DELETE

        public async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter = Builders<UserRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "weeklyViewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<UserRecommendationsDocument>.Update.Pull("weeklyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateManyAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }

        

    }
}