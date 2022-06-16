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
        private readonly string _playlistCollectionName;

        public PlaylistRecommendationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _recommendationsCollection =
                mongoDatabase.GetCollection<PlaylistRecommendationsDocument>(databaseSettings.Value
                    .PlaylistRecommendationsCollectionName);

            _playlistCollectionName = databaseSettings.Value.PlaylistsCollectionName;
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

        public async Task<List<BsonDocument>?> GetTrendingPlaylists(string playlistNameBeginningLetters, int resultsLimit)
        {
            var query = await _recommendationsCollection.Aggregate()
                .SortByDescending(p => p.WeeklyViewDates)
                .Lookup(_playlistCollectionName, "playlistId", "_id", "playlist")
                .Unwind("playlist")
                .Match(Builders<BsonDocument>.Filter
                    .Regex("title", new BsonRegularExpression("(?i)^" + playlistNameBeginningLetters)))
                .Limit(resultsLimit).
                ToListAsync();
            
            return query;

        }


        public async Task SaveNewPlaylistView(SavePlaylistViewDto request)
        {
            var recommendationsDocument = new PlaylistRecommendationsDocument(request);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }

        public async Task AddViewToPlaylist(string playlistId,
            int currentWeeklyViewAmount,
            int totalViewAmount
        )
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var weeklyViewsUpdate = Builders<PlaylistRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }

        public async Task<PlaylistRecommendationsDocument?> GetPlaylistRecommendationsDocumentById(string playlistId)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var result = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return result;
        }
    }
}