﻿using System.Diagnostics;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;


namespace SkyPlaylistManager.Services
{
    public class PlaylistRecommendationsService
    {
        private readonly IMongoCollection<PlaylistRecommendationsDocument> _recommendationsCollection;
        private readonly IMongoCollection<PlaylistDocument> _playlistsCollection;

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


        // CREATE

        public async Task SaveNewPlaylistView(string playlistId)
        {
            var recommendationsDocument = new PlaylistRecommendationsDocument(playlistId);
            await _recommendationsCollection.InsertOneAsync(recommendationsDocument);
        }


        // READ

        public async Task<List<GetTrendingPlaylistsLookupDto>?> GetTrendingPlaylists(
            string playlistNameBeginningLetters, int resultsLimit)
        {
            var trendingPlaylists = await _recommendationsCollection.Aggregate()
                .Lookup(_playlistCollectionName, "playlistId", "_id", "playlist")
                .Unwind("playlist")
                .Match(Builders<BsonDocument>.Filter
                    .Regex("playlist.title", new BsonRegularExpression("(?i)^" + playlistNameBeginningLetters)))
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

        public async Task<PlaylistRecommendationsDocument?> GetPlaylistRecommendationsDocumentById(string playlistId)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var result = await _recommendationsCollection.Find(filter).FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<PlaylistRecommendationsDocument>?> GetPlaylistsRecommendationsDocumentsByIds(
            IEnumerable<string> playlistId)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.In(p => p.PlaylistId, playlistId);

            var result = await _recommendationsCollection.Find(filter).ToListAsync();

            return result;
        }

        public async Task<int> GetAmountTotalPlaylistViews(List<ObjectId> userPlaylistIds)
        {
            int totalViews = 0;

            var playlistIdsString = userPlaylistIds.Select(p => p.ToString());

            var playlists = await GetPlaylistsRecommendationsDocumentsByIds(playlistIdsString);
            if (playlists == null) return totalViews;

            foreach (var playlist in playlists)
            {
                totalViews += playlist.TotalViewsAmount;
            }

            return totalViews;
        }

        public async Task<int> GetAmountWeeklyPlaylistViews(List<ObjectId> userPlaylistIds)
        {
            int totalViews = 0;

            var playlistIdsString = userPlaylistIds.Select(p => p.ToString());

            var playlists = await GetPlaylistsRecommendationsDocumentsByIds(playlistIdsString);
            if (playlists == null) return totalViews;

            foreach (var playlist in playlists)
            {
                totalViews += playlist.WeeklyViewDates.Count;
            }
            return totalViews;
        }


        // UPDATE
        
        public async Task AddViewToPlaylist(string playlistId, int totalViewAmount)
        {
            var filter = Builders<PlaylistRecommendationsDocument>.Filter.Eq(p => p.PlaylistId, playlistId);

            var weeklyViewsUpdate = Builders<PlaylistRecommendationsDocument>.Update
                .Push(p => p.WeeklyViewDates, DateTime.Now)
                .Set(p => p.TotalViewsAmount, totalViewAmount + 1);
            await _recommendationsCollection.UpdateOneAsync(filter, weeklyViewsUpdate);
        }


        // DELETE

        public async void DeleteOldRecommendations()
        {
            var recommendationsWithOldDatesFilter =
                Builders<PlaylistRecommendationsDocument>.Filter.ElemMatch<BsonValue>(
                    "weeklyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            var pullOldDates =
                Builders<PlaylistRecommendationsDocument>.Update.Pull("weeklyViewDates",
                    new BsonDocument("$lt", DateTime.Now.AddDays(-7)));

            await _recommendationsCollection.UpdateManyAsync(recommendationsWithOldDatesFilter, pullOldDates);
        }
    }
}