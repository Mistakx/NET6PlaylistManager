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

        public async Task GetTrending()
        {
            // var aggregation = _recommendationsCollection.Aggregate().Project(new BsonDocument(
            //     "count", new BsonDocument("$sum", "$viewDates")));
            // // .Sort(new BsonDocument("count", -1));
            //
            // var numberOfDates = await aggregation.FirstOrDefaultAsync();
            //
            //
            // var update = Builders<RecommendationsDocument>.Update.PullFilter("followerList",
            //     Builders<List<DateTime>>.Filter.Eq("follower", "fethiye"));
            //
            // var deleteFilter2 = Builders<RecommendationsDocument>.Filter.ElemMatch<BsonValue>("viewDates",
            //     new BsonDocument("$lt", DateTime.Now.AddDays(-7)));
            //
            // var update2 = Builders<RecommendationsDocument>.Update.PullFilter("viewDates",
            //     new BsonDocument("$lt", DateTime.Now.AddDays(-7)));


            var recommendationsWithOldDatesFilter = Builders<RecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                "viewDates",
                new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<RecommendationsDocument>.Update.Pull("viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateOneAsync(recommendationsWithOldDatesFilter, pullOldDates);

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