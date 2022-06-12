using Microsoft.Extensions.Options;
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
            string platformPlayerUrl)
        {
            var builder = Builders<RecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var update = Builders<RecommendationsDocument>.Update.Push(p => p.Views, DateTime.Now);

            await _recommendationsCollection.UpdateOneAsync(filter, update);
        }
    }
}

