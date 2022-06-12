using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;


namespace SkyPlaylistManager.Services
{
    public class RecommendationsService
    {
        private readonly IMongoCollection<RecommendationsDocument> _recommendationsCollection;

        public RecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<RecommendationsDocument>(databaseSettings.Value
                    .RecommendationsCollectionName);
        }

        private async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter = Builders<RecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "viewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<RecommendationsDocument>.Update.Pull("viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateOneAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }

        public async void UpdateRecommendationsWeeklyViews()
        {
            var recommendationsWithOldDatesFilter = Builders<RecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "viewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var recommendationWithOldDates =
                await _recommendationsCollection.Find(recommendationsWithOldDatesFilter).ToListAsync();

            DeleteOldRecommendations();

            foreach (var recommendation in recommendationWithOldDates)
            {
                var recommendationWithNewDates =
                    await _recommendationsCollection.Find(
                            Builders<RecommendationsDocument>.Filter.Eq("_id", new ObjectId(recommendation.Id)))
                        .FirstOrDefaultAsync();

                await _recommendationsCollection.UpdateOneAsync(p => p.Id == recommendation.Id,
                    Builders<RecommendationsDocument>.Update.Set("viewsAmount",
                        recommendationWithNewDates.WeeklyViewDates.Count));
            }
        }

        public async Task<List<BsonDocument>> GetTrending()
        {
            
            var projection = Builders<RecommendationsDocument>.Projection
                .Include("generalizedResult")
                .Exclude("_id");

            
            var trendingPlaylists = await _recommendationsCollection.Find(p => true)
                .Project(projection)
                .SortByDescending(p => p.WeeklyViewsAmount)
                .Limit(10).ToListAsync();

            return trendingPlaylists;
        }


        public async Task SaveView(SaveViewDto request)
        {
            var recommendationsDocument = new RecommendationsDocument(request);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }

        public async Task<RecommendationsDocument?> GetResultInRecommended(
            string title,
            string playerFactoryName,
            string platformPlayerUrl)
        {
            var builder = Builders<RecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var recommendation = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return recommendation;
        }

        public async Task AddViewToResult(
            string title,
            string playerFactoryName,
            string platformPlayerUrl,
            int currentWeeklyViewAmount,
            int totalViewAmount
        )
        {
            var builder = Builders<RecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var weeklyViewsUpdate = Builders<RecommendationsDocument>.Update.Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.WeeklyViewsAmount, currentWeeklyViewAmount + 1)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }

        public async Task<BsonDocument?> GetViews(string playerFactoryName,
            string platformId,
            string? platformPlayerUrl)
        {
            var filter = Builders<RecommendationsDocument>.Filter.Eq(p => p.GeneralizedResult.PlatformId, platformId) &
                         Builders<RecommendationsDocument>.Filter.Eq(p => p.GeneralizedResult.PlayerFactoryName,
                             playerFactoryName) &
                         Builders<RecommendationsDocument>.Filter.Eq(p => p.GeneralizedResult.PlatformPlayerUrl,
                             platformPlayerUrl);

            var projection = Builders<RecommendationsDocument>.Projection
                .Include("weeklyViewsAmount")
                .Include("totalViewsAmount")
                .Exclude("_id");

            var result = await _recommendationsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }
    }
}