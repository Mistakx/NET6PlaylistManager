using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;


namespace SkyPlaylistManager.Services
{
    public class ContentRecommendationsService
    {
        private readonly IMongoCollection<ContentRecommendationsDocument> _recommendationsCollection;

        public ContentRecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<ContentRecommendationsDocument>(databaseSettings.Value
                    .ContentRecommendationsCollectionName);
        }

        
        // CREATE

        public async Task SaveView(SaveContentViewDto request)
        {
            var recommendationsDocument = new ContentRecommendationsDocument(request);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }


        // READ

        public async Task<List<ContentRecommendationsDocument>?> GetTrendingContent(int limit)
        {
            var trendingContentList = await _recommendationsCollection.Find(p => true).ToListAsync();
            trendingContentList.Sort((x, y) => y.WeeklyViewDates.Count.CompareTo(x.WeeklyViewDates.Count));
            return trendingContentList.Take(limit).ToList();
        }

        public async Task<ContentRecommendationsDocument?> GetContentInRecommendedCollection(string title, string playerFactoryName,
            string platformPlayerUrl)
        {
            var builder = Builders<ContentRecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var recommendation = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();
            return recommendation;
        }


        // UPDATE

        public async void UpdateRecommendationsWeeklyViews()
        {
            var recommendationsWithOldDatesFilter =
                Builders<ContentRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var recommendationWithOldDates =
                await _recommendationsCollection.Find(recommendationsWithOldDatesFilter).ToListAsync();

            DeleteOldRecommendations();

            foreach (var recommendation in recommendationWithOldDates)
            {
                var recommendationWithNewDates =
                    await _recommendationsCollection.Find(
                            Builders<ContentRecommendationsDocument>.Filter.Eq("_id", new ObjectId(recommendation.Id)))
                        .FirstOrDefaultAsync();

                await _recommendationsCollection.UpdateOneAsync(p => p.Id == recommendation.Id,
                    Builders<ContentRecommendationsDocument>.Update.Set("viewsAmount",
                        recommendationWithNewDates.WeeklyViewDates.Count));
            }
        }

        public async Task AddViewToResult(string title, string playerFactoryName, string platformPlayerUrl,
            int totalViewAmount)
        {
            var builder = Builders<ContentRecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var weeklyViewsUpdate = Builders<ContentRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }


        // DELETE

        private async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<ContentRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<ContentRecommendationsDocument>.Update.Pull("viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateOneAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }
    }
}