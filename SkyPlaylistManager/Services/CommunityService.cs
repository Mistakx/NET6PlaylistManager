using System.Collections;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Services
{
    public class CommunityService
    {
        private readonly IMongoCollection<PlaylistDocument> _playlistsCollection;
        private readonly IMongoCollection<UserDocument> _userCollection;

        public CommunityService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _userCollection =
                mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value
                    .UsersCollectionName);

            _playlistsCollection = mongoDatabase.GetCollection<PlaylistDocument>(databaseSettings.Value
                .PlaylistsCollectionName);
        }


        // READ
        
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

        public async Task<bool> UserAlreadyBeingFollowed(string userToFollowId, string userFollowingId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userToFollowId);
            var followedUser = await _userCollection.Find(filter).FirstOrDefaultAsync();
            var userIsAlreadyBeingFollowed = followedUser.UsersFollowingIds.Contains(new ObjectId(userFollowingId));
            return userIsAlreadyBeingFollowed;
        }

        public async Task<bool> PlaylistAlreadyBeingFollowed(string playlistId, string userId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var playlist = await _playlistsCollection.Find(filter).FirstOrDefaultAsync();
            var playlistIsAlreadyBeingFollowedByUser = playlist.UsersFollowingIds.Contains(new ObjectId(userId));
            return playlistIsAlreadyBeingFollowedByUser;
        }
        
        public async Task<List<PlaylistDocument>?> GetFollowedPlaylists(string userId)
        {
            var filter = Builders<PlaylistDocument>.Filter.AnyIn(p => p.UsersFollowingIds, new List<ObjectId>{new ObjectId(userId)});
            var followedPlaylists = await _playlistsCollection.Find(filter).ToListAsync();
            return followedPlaylists;
        }

        public async Task<List<UserDocument>?> GetFollowedUsers(string userId)
        {
            var filter = Builders<UserDocument>.Filter.AnyIn(p => p.UsersFollowingIds, new List<ObjectId>{new ObjectId(userId)});
            var followedUsers = await _userCollection.Find(filter).ToListAsync();
            return followedUsers;
        }

        // UPDATE
        
        public async Task FollowPlaylist(string playlistToUnfollowId, ObjectId userUnfollowingId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistToUnfollowId);
            var update = Builders<PlaylistDocument>.Update.Push("usersFollowingIds", userUnfollowingId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task FollowUser(string userToUnfollowId, ObjectId userFollowingId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userToUnfollowId);
            var update = Builders<UserDocument>.Update.Push("usersFollowingIds", userFollowingId);

            await _userCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UnfollowPlaylist(string playlistToUnfollowId, ObjectId userUnfollowingId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistToUnfollowId);
            var update = Builders<PlaylistDocument>.Update.Pull("usersFollowingIds", userUnfollowingId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UnfollowUser(string userToUnfollowId, ObjectId userFollowingId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userToUnfollowId);
            var update = Builders<UserDocument>.Update.Pull("usersFollowingIds", userFollowingId);

            await _userCollection.UpdateOneAsync(filter, update);
        }
    }
}