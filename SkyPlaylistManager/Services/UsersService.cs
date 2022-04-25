using System.Text.Json;
using AutoMapper;
using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;


namespace SkyPlaylistManager.Services
{
    public class UsersService
    {

        private readonly IMongoCollection<UserCollection> _usersCollection;


        public UsersService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<UserCollection>(("Users"));
        }


        public async Task<List<BsonDocument>> GetUserPlaylists(string userId)
        {
           
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var query = _usersCollection.Aggregate().Match(filter)
                .Lookup("Playlists", "userPlaylists", "_id", "userPlaylists");

            List<BsonDocument> result = await query.ToListAsync();

            return result;
        }





        public async Task<List<UserCollection>> GetAllUsers()
        {
            var result = await _usersCollection.Find(_ => true).ToListAsync();
            return result;
        }


        public async Task CreateUser(UserCollection newUserCollection) =>
            await _usersCollection.InsertOneAsync(newUserCollection);

        public async Task<UserCollection?> GetUserById(string userId) =>
            await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();


        public async Task<UserCollection?> GetUserByEmail(string email) =>
            await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

        



    }
}