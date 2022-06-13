using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;


namespace SkyPlaylistManager.Services
{
    public class PlaylistRecommendationsService
    {
        private readonly IMongoCollection<PlaylistRecommendationsDocument> _recommendationsCollection;

        public PlaylistRecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<PlaylistRecommendationsDocument>(databaseSettings.Value
                    .RecommendationsCollectionName);
        }

        private async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<PlaylistRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<PlaylistRecommendationsDocument>.Update.Pull("viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateOneAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }

        public async void UpdateRecommendationsWeeklyViews()
        {
            var recommendationsWithOldDatesFilter =
                Builders<PlaylistRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "viewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var recommendationWithOldDates =
                await _recommendationsCollection.Find(recommendationsWithOldDatesFilter).ToListAsync();

            DeleteOldRecommendations();

            foreach (var recommendation in recommendationWithOldDates)
            {
                var recommendationWithNewDates =
                    await _recommendationsCollection.Find(
                            Builders<PlaylistRecommendationsDocument>.Filter.Eq("_id", new ObjectId(recommendation.Id)))
                        .FirstOrDefaultAsync();

                await _recommendationsCollection.UpdateOneAsync(p => p.Id == recommendation.Id,
                    Builders<PlaylistRecommendationsDocument>.Update.Set("viewsAmount",
                        recommendationWithNewDates.WeeklyViewDates.Count));
            }
        }

        public async Task<List<BsonDocument>> GetTrendingPlaylistIds(string playlistNameBeginningLetters)
        {
            var projection = Builders<PlaylistRecommendationsDocument>.Projection
                .Include("playlistId")
                .Exclude("_id");

            var trendingPlaylists = await _recommendationsCollection.Find(p =>
                    p.PlaylistName.ToLower().StartsWith(playlistNameBeginningLetters.ToLower()))
                .Project(projection)
                .SortByDescending(p => p.WeeklyViewsAmount)
                .Limit(10).ToListAsync();

            return trendingPlaylists;
        }


        public async Task SaveView(SavePlaylistViewDto request)
        {
            var recommendationsDocument = new PlaylistRecommendationsDocument(request);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }

        public async Task<PlaylistRecommendationsDocument?> GetResultInRecommended(string playlistId)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var recommendation = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return recommendation;
        }

        public async Task AddViewToResult(string playlistId,
            int currentWeeklyViewAmount,
            int totalViewAmount
        )
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var weeklyViewsUpdate = Builders<PlaylistRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.WeeklyViewsAmount, currentWeeklyViewAmount + 1)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }

        public async Task<BsonDocument?> GetViews(string playlistId)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);


            var projection = Builders<PlaylistRecommendationsDocument>.Projection
                .Include("weeklyViewsAmount")
                .Include("totalViewsAmount")
                .Exclude("_id");

            var result = await _recommendationsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }
    }
}