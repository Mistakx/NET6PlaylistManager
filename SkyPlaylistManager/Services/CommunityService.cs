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
        private readonly IMongoCollection<UserDocument> _usersCollection;

        public CommunityService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value.UsersCollectionName);

            _playlistsCollection = mongoDatabase.GetCollection<PlaylistDocument>(databaseSettings.Value
                .PlaylistsCollectionName);
        }


        // READ

        public async Task<List<UserDocument>> GetUsersByNameOrUsername(string username, int limitAmount)
        {
            var userDocuments = new List<UserDocument>();

            var filter = Builders<UserDocument>.Filter.Regex("name", new BsonRegularExpression("(?i)^" + username));
            var result = await _usersCollection.Find(filter).Limit(limitAmount).ToListAsync();
            foreach (var user in result)
            {
                userDocuments.Add(user);
            }

            filter = Builders<UserDocument>.Filter.Regex("username", new BsonRegularExpression("(?i)^" + username));
            result = await _usersCollection.Find(filter).Limit(limitAmount).ToListAsync();
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

        public async Task<bool> UserAlreadyBeingFollowed(string userFollowingId, string userToFollowId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userFollowingId);
            var userFollowing = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return userFollowing.FollowingUsersIds.Contains(new ObjectId(userToFollowId));
        }

        public async Task<bool> PlaylistAlreadyBeingFollowed(string playlistId, string userId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return user.FollowingPlaylistsIds.Contains(new ObjectId(playlistId));
        }

        public async Task<List<PlaylistDocument>?> GetFollowedPlaylists(string userId)
        {
            var userFilter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var user = await _usersCollection.Find(userFilter).FirstOrDefaultAsync();

            var playlistDocuments = new List<PlaylistDocument>();
            foreach (var playlistId in user.FollowingPlaylistsIds)
            {
                var playlistFilter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId.ToString());
                var playlist = await _playlistsCollection.Find(playlistFilter).FirstOrDefaultAsync();
                playlistDocuments.Add(playlist);
            }

            return playlistDocuments;
        }

        public async Task<List<UserDocument>?> GetFollowedUsers(string userId)
        {
            // var filter =
            //     Builders<UserDocument>.Filter.AnyIn(p => p.UsersFollowingIds,
            //         new List<ObjectId> {new ObjectId(userId)});
            // var followedUsers = await _userCollection.Find(filter).ToListAsync();
            // return followedUsers;
            //
            var followingUserFilter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var user = await _usersCollection.Find(followingUserFilter).FirstOrDefaultAsync();

            var followedUserDocuments = new List<UserDocument>();
            foreach (var followedUserId in user.FollowingUsersIds)
            {
                var followedUsersFilter = Builders<UserDocument>.Filter.Eq(p => p.Id, followedUserId.ToString());
                var followedUserDocument = await _usersCollection.Find(followedUsersFilter).FirstOrDefaultAsync();
                followedUserDocuments.Add(followedUserDocument);
            }

            return followedUserDocuments;
        }

        public async Task<List<UserDocument>?> GetUsersFollowingUser(string userId)
        {
            // var filter = Builders<PlaylistDocument>.Filter.AnyIn(p => p.UsersFollowingIds,
            //     new List<ObjectId> {new ObjectId(userId)});
            var usersFollowingUser = await _usersCollection
                .Find(u => u.FollowingUsersIds.Contains(ObjectId.Parse(userId))).ToListAsync();
            return usersFollowingUser;
        }
        
        public async Task<List<UserDocument>?> GetUsersFollowingPlaylist(string playlistId)
        {
            // var filter = Builders<PlaylistDocument>.Filter.AnyIn(p => p.UsersFollowingIds,
            //     new List<ObjectId> {new ObjectId(userId)});
            var usersFollowingPlaylist = await _usersCollection
                .Find(u => u.FollowingPlaylistsIds.Contains(ObjectId.Parse(playlistId))).ToListAsync();
            return usersFollowingPlaylist;
        }

        public async Task<int> GetUserFollowersAmount(string userId)
        {
            var userFilter =
                Builders<UserDocument>.Filter.AnyIn(p => p.FollowingUsersIds,
                    new List<ObjectId> {new ObjectId(userId)});
            var followers = await _usersCollection.Find(userFilter).ToListAsync();
            return followers.Count;
        }
        
        public async Task<int> GetPlaylistFollowersAmount(string playlistId)
        {
            var userFilter =
                Builders<UserDocument>.Filter.AnyIn(p => p.FollowingPlaylistsIds,
                    new List<ObjectId> {new ObjectId(playlistId)});
            var followers = await _usersCollection.Find(userFilter).ToListAsync();
            return followers.Count;
        }


        // UPDATE

        public async Task FollowPlaylist(string userFollowingId, ObjectId playlistToFollowId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userFollowingId);
            var update = Builders<UserDocument>.Update.Push("followingPlaylistsIds", playlistToFollowId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task FollowUser(string userFollowingId, ObjectId userToFollowId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userFollowingId);
            var update = Builders<UserDocument>.Update.Push("followingUsersIds", userToFollowId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UnfollowPlaylist(string userUnfollowingId, ObjectId playlistToUnfollowId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userUnfollowingId);
            var update = Builders<UserDocument>.Update.Pull("followingPlaylistsIds", playlistToUnfollowId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UnfollowUser(string userUnfollowingId, ObjectId userToUnfollowId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userUnfollowingId);
            var update = Builders<UserDocument>.Update.Pull("followingUsersIds", userToUnfollowId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertFollowedUserIdInSpecificPosition(string followingUserId, int newIndex, string followedUserId)
        {
            var followedUserIds = new List<ObjectId>(); // The method "PushEach" only works with lists
            var generalizedResultId = ObjectId.Parse(followedUserId);
            followedUserIds.Add(generalizedResultId);

            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, followingUserId);
            var update =
                Builders<UserDocument>.Update.PushEach("followingUsersIds", followedUserIds,
                    position: newIndex);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertFollowedPlaylistIdInSpecificPosition(string followingUserId, int newIndex,
            string followedPlaylistId)
        {
            var followedPlaylistIds = new List<ObjectId>(); // The method "PushEach" only works with lists
            var generalizedResultId = ObjectId.Parse(followedPlaylistId);
            followedPlaylistIds.Add(generalizedResultId);

            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, followingUserId);
            var update =
                Builders<UserDocument>.Update.PushEach("followingPlaylistsIds", followedPlaylistIds,
                    position: newIndex);

            await _usersCollection.UpdateOneAsync(filter, update);
        }


        // DELETE

        public async Task DeleteFollowedUserId(string followingUserId, ObjectId followedUserId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, followingUserId);
            var update = Builders<UserDocument>.Update.Pull("followingUsersIds", followedUserId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteFollowedPlaylistId(string followingUserId, ObjectId followedPlaylistId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, followingUserId);
            var update = Builders<UserDocument>.Update.Pull("followingPlaylistsIds", followedPlaylistId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
    }
}