using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;


namespace SkyPlaylistManager.Services
{
    public class UserRecommendationsService
    {
        private readonly IMongoCollection<UserRecommendationsDocument> _recommendationsCollection;

        public UserRecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<UserRecommendationsDocument>(databaseSettings.Value
                    .UserRecommendationsCollectionName);
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

        // public async Task<List<BsonDocument>> GetTrendingUserIds(string usernameBeginningLetters)
        // {
        //     var projection = Builders<UserRecommendationsDocument>.Projection
        //         .Include("userId")
        //         .Exclude("_id");
        //
        //
        //     var trendingPlaylists = await _recommendationsCollection.Find(p => p.Username.ToLower().StartsWith(usernameBeginningLetters.ToLower()))
        //         .Project(projection)
        //         .SortByDescending(p => p.WeeklyViewsAmount)
        //         .Limit(10).ToListAsync();
        //
        //     return trendingPlaylists;
        // }


        public async Task SaveView(SaveUserViewDto request)
        {
            var recommendationsDocument = new UserRecommendationsDocument(request);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }

        public async Task<UserRecommendationsDocument?> GetResultInRecommended(string userId)
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);

            var recommendation = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return recommendation;
        }

        public async Task AddViewToResult(
            string userId,
            int currentWeeklyViewAmount,
            int totalViewAmount
        )
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);


            var weeklyViewsUpdate = Builders<UserRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.WeeklyViewsAmount, currentWeeklyViewAmount + 1)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }

        public async Task<BsonDocument?> GetViews(string userId)
        {
            var filter = Builders<UserRecommendationsDocument>.Filter.Eq(p => p.UserId, userId);
            
            var projection = Builders<UserRecommendationsDocument>.Projection
                .Include("weeklyViewsAmount")
                .Include("totalViewsAmount")
                .Exclude("_id");

            var result = await _recommendationsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }
    }
}