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

        
        public async Task<List<ContentRecommendationsDocument>?> GetTrendingMonthlyContent(int limit)
        {
            var trendingContentList = await _recommendationsCollection.Find(p => p.MonthlyViewDates.Count > 0).ToListAsync();
            trendingContentList.Sort((x, y) => y.MonthlyViewDates.Count.CompareTo(x.MonthlyViewDates.Count));
            return trendingContentList.Take(limit).ToList();
        }

        public async Task<List<ContentRecommendationsDocument>?> GetTrendingWeeklyContent(int limit)
        {
            var trendingContentList = await _recommendationsCollection.Find(p => p.WeeklyViewDates.Count > 0).ToListAsync();
            trendingContentList.Sort((x, y) => y.WeeklyViewDates.Count.CompareTo(x.WeeklyViewDates.Count));
            return trendingContentList.Take(limit).ToList();
        }
        
        public async Task<List<ContentRecommendationsDocument>?> GetTrendingDailyContent(int limit)
        {
            var trendingContentList = await _recommendationsCollection.Find(p => p.DailyViewDates.Count > 0).ToListAsync();
            trendingContentList.Sort((x, y) => y.DailyViewDates.Count.CompareTo(x.DailyViewDates.Count));
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

        public async Task AddViewToResult(string title, string playerFactoryName, string platformPlayerUrl,
            int totalViewAmount)
        {
            var builder = Builders<ContentRecommendationsDocument>.Filter;
            var filter = builder.Eq(p => p.GeneralizedResult.Title, title) &
                         builder.Eq(p => p.GeneralizedResult.PlayerFactoryName, playerFactoryName) &
                         builder.Eq(p => p.GeneralizedResult.PlatformPlayerUrl, platformPlayerUrl);

            var weeklyViewsUpdate = Builders<ContentRecommendationsDocument>.Update
                .Push(p => p.MonthlyViewDates, DateTime.Now)
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Push(p => p.DailyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }


        // DELETE
        public async void DeleteMonthlyOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<ContentRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "monthlyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-30)));

            var test = await _recommendationsCollection.Find(recommendationsWithOldDatesFilter).ToListAsync();
            
            var pullOldDates =
                Builders<ContentRecommendationsDocument>.Update.Pull("monthlyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-30)));

            await _recommendationsCollection.UpdateManyAsync(recommendationsWithOldDatesFilter, pullOldDates);
            
            
        }
        public async void DeleteWeeklyOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<ContentRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "weeklyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<ContentRecommendationsDocument>.Update.Pull("weeklyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateManyAsync(recommendationsWithOldDatesFilter, pullOldDates);
            
            
        }
        public async void DeleteDailyOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<ContentRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "dailyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-1)));

            var pullOldDates =
                Builders<ContentRecommendationsDocument>.Update.Pull("dailyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-1)));

            await _recommendationsCollection.UpdateManyAsync(recommendationsWithOldDatesFilter, pullOldDates);
            
            
        }

    }
}