﻿using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;


namespace SkyPlaylistManager.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<UserDocument> _usersCollection;
        private readonly string _playlistsCollectionName;

        public UsersService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value.UsersCollectionName);
            _playlistsCollectionName = databaseSettings.Value.PlaylistsCollectionName;
        }
        
        public async Task CreateUser(UserDocument newUserDocument) =>
            await _usersCollection.InsertOneAsync(newUserDocument);

        public async Task<UserDocument?> GetUserById(string userId) =>
            await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        public async Task<UserDocument?> GetUserByUsername(string username) =>
            await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

        public async Task<UserDocument?> GetUserByEmail(string email) =>
            await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        
        public async Task UpdateUserProfilePhoto(string userId, string photoPath)
        {
            var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);
            var update = Builders<UserDocument>.Update.Set("profilePhotoUrl", photoPath);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdatePassword(string userId, string newPassword)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserDocument>.Update.Set("password", newPassword);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateEmail(string userId, string newEmail)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserDocument>.Update.Set("email", newEmail);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateName(string userId, string newName)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var updated = Builders<UserDocument>.Update.Set("name", newName);

            await _usersCollection.UpdateOneAsync(filter, updated);
        }

        public async Task UpdateUsername(string userId, string newUsername)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var updated = Builders<UserDocument>.Update.Set("username", newUsername);

            await _usersCollection.UpdateOneAsync(filter, updated);
        }

        public async Task DeleteUserPlaylist(string userId, ObjectId playlist)
        {
            var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);
            var update = Builders<UserDocument>.Update.Pull("playlistIds", playlist);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task<BsonDocument> GetUserPlaylists(string userId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var query = _usersCollection.Aggregate().Match(filter)
                .Lookup(_playlistsCollectionName, "playlistIds", "_id", "playlists");
            
            var result = await query.FirstOrDefaultAsync();
            return result;
        }

    }
}