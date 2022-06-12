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

        public async void UpdateRecommendationsViews()
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
                        Builders<RecommendationsDocument>.Filter.Eq("_id", new ObjectId(recommendation.Id))).FirstOrDefaultAsync();
                
                await _recommendationsCollection.UpdateOneAsync(p => p.Id == recommendation.Id,
                    Builders<RecommendationsDocument>.Update.Set("viewsAmount",
                        recommendationWithNewDates.ViewDates.Count));
            }
        }

        public async Task<List<RecommendationsDocument>?> GetTrending()
        {
            var trendingPlaylists = await _recommendationsCollection.Find(p => true)
                .SortByDescending(p => p.ViewsAmount)
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

        public async Task AddViewToResultInRecommended(
            string title,
            string playerFactoryName,
            string platformPlayerUrl,
            int currentViewAmount)
        {
            var builder = Builders<RecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var update = Builders<RecommendationsDocument>.Update.Push(p => p.ViewDates, DateTime.Now)
                .Set(p => p.ViewsAmount, currentViewAmount + 1);

            await _recommendationsCollection.UpdateOneAsync(filter, update);
        }
    }
}