﻿using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Services
{
    public class CommunityService
    {
        private readonly IMongoCollection<UserRecommendationsDocument> _recommendationsCollection;
        private readonly IMongoCollection<PlaylistDocument> _playlistsCollection;
        private readonly IMongoCollection<UserDocument> _userCollection;
        private readonly string _userCollectionName;

        public CommunityService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _userCollectionName = databaseSettings.Value.UsersCollectionName;

            _recommendationsCollection =
                mongoDatabase.GetCollection<UserRecommendationsDocument>(databaseSettings.Value
                    .UserRecommendationsCollectionName);

            _userCollection =
                mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value
                    .UsersCollectionName);

            _playlistsCollection = mongoDatabase.GetCollection<PlaylistDocument>(databaseSettings.Value
                .PlaylistsCollectionName);
        }

        public async Task<List<UserDocument>> GetUsersByNameOrUsername(string username, int limitAmount)
        {
            var userDocuments = new List<UserDocument>();

            var filter = Builders<UserDocument>.Filter.Regex("name", new BsonRegularExpression("(?i)^" + username));
            var result = await _userCollection.Find(filter).Limit(limitAmount).ToListAsync();
            foreach (var user in result)
            {
                userDocuments.Add(user);
            }

            filter = Builders<UserDocument>.Filter.Regex("username", new BsonRegularExpression("(?i)^" + username));
            result = await _userCollection.Find(filter).Limit(limitAmount).ToListAsync();
            foreach (var user in result)
            {
                userDocuments.Add(user);
            }

            return userDocuments.GroupBy(userDocument => userDocument.Username)
                .Select(group => group.First()).ToList();
        }

        // Playlists
        public async Task<List<PlaylistDocument>> GetPlaylistsByTitle(string title, int limitAmount)
        {
            var playlistDocuments = new List<PlaylistDocument>();

            var filter = Builders<PlaylistDocument>.Filter.Regex("title", new BsonRegularExpression("(?i)^" + title));
            var result = await _playlistsCollection.Find(filter).Limit(limitAmount).ToListAsync();
            foreach (var user in result)
            {
                playlistDocuments.Add(user);
            }

            return playlistDocuments;
        }

        public async Task<bool> PlaylistAlreadyBeingFollowedByUser(string playlistId, string userId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var playlist = await _playlistsCollection.Find(filter).FirstOrDefaultAsync();
            var playlistIsAlreadyBeingFollowedByUser = playlist.UsersFollowingIds.Contains(new ObjectId(userId));
            return playlistIsAlreadyBeingFollowedByUser;
        }

        public async Task FollowPlaylist(string playlistToUnfollowId, ObjectId userUnfollowingId,
            int currentUsersFollowingAmount)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistToUnfollowId);
            var update = Builders<PlaylistDocument>.Update.Pull("usersFollowingIds", userUnfollowingId)
                .Set("resultsAmount", currentUsersFollowingAmount + 1);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task UnfollowPlaylist(string playlistToUnfollowId, ObjectId userUnfollowingId,
            int currentUsersFollowingAmount)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistToUnfollowId);
            var update = Builders<PlaylistDocument>.Update.Pull("usersFollowingIds", userUnfollowingId)
                .Set("resultsAmount", currentUsersFollowingAmount - 1);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }
    }
}