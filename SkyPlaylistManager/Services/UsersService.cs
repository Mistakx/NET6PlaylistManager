using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;



namespace SkyPlaylistManager.Services
{
    public class UsersService
    {

        private readonly IMongoCollection<User> _usersCollection;


        public UsersService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(("Users"));   
        }


        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(User newBook) =>
            await _usersCollection.InsertOneAsync(newBook);

        public async Task<User?> GetAsyncById(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        //public async Task<List<User>> GetAsyncByEmail(string email) =>
        //    await _usersCollection.Find(x => x.Email == email).ToListAsync();

        public async Task<User?> GetAsyncByEmail(string email) =>
            await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();


    }
}
