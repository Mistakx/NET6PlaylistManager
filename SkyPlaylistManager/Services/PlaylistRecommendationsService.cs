using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;


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

        public async Task<List<GetTrendingPlaylistsLookupDto>?> GetTrendingPlaylists(
            string playlistNameBeginningLetters,
            int resultsLimit)
        {
            var trendingPlaylists = await _recommendationsCollection.Aggregate()
                .Lookup(_playlistCollectionName, "playlistId", "_id", "playlist")
                .Unwind("playlist")
                .Match(Builders<BsonDocument>.Filter
                    .Regex("title", new BsonRegularExpression("(?i)^" + playlistNameBeginningLetters)))
                .ToListAsync();

            var deserializedTrendingPlaylists = new List<GetTrendingPlaylistsLookupDto>();
            foreach (var trendingPlaylist in trendingPlaylists)
            {
                var deserializedTrendingPlaylist =
                    BsonSerializer.Deserialize<GetTrendingPlaylistsLookupDto>(trendingPlaylist);
                deserializedTrendingPlaylists.Add(deserializedTrendingPlaylist);
            }

            deserializedTrendingPlaylists.Sort((x, y) => y.WeeklyViewDates.Count.CompareTo(x.WeeklyViewDates.Count));
            return deserializedTrendingPlaylists.Take(resultsLimit).ToList();
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