using Microsoft.Extensions.Options;
using MongoDB.Bson;
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


        public async Task<BsonDocument> GetUserDetailsAndPlaylists(string userId) // TODO: está a retornar apenas uma playlist
        {

            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var query = _usersCollection.Aggregate().Match(filter)
                .Lookup("Playlists", "userPlaylists", "_id", "userPlaylists")

                .Project(Builders<BsonDocument>.Projection.Exclude("password"))
                .Project(Builders<BsonDocument>.Projection.Exclude("userPlaylists.owner")
                    .Exclude("userPlaylists.sharedWith")
                    .Exclude("userPlaylists.contents"));

            //var result = await query.FirstOrDefaultAsync();
            List<BsonDocument> result = await query.ToListAsync();

            return result[0];
        }
        
        public async Task<List<BsonDocument>> GetUserPlaylists(string userId)
        {
           
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var query = _usersCollection.Aggregate().Match(filter)
                .Lookup("Playlists", "userPlaylists", "_id", "userPlaylists")

                .Project(Builders<BsonDocument>.Projection.Exclude("password"))
                .Project(Builders<BsonDocument>.Projection.Exclude("userPlaylists.owner")
                    .Exclude("userPlaylists.sharedWith")
                    .Exclude("userPlaylists.contents"));

            List<BsonDocument> result = await query.ToListAsync();

            return result;
        }
        
        public async Task<List<BsonDocument>> GetUserNamePlaylists(string username)
        {

            var filter = Builders<UserCollection>.Filter.Eq(u => u.Username, username);
            var query = _usersCollection.Aggregate().Match(filter)
                .Lookup("Playlists", "userPlaylists", "_id", "userPlaylists")

                .Project(Builders<BsonDocument>.Projection.Exclude("password"))
                .Project(Builders<BsonDocument>.Projection.Exclude("userPlaylists.owner")
                    .Exclude("userPlaylists.sharedWith")
                    .Exclude("userPlaylists.sharedWith")
                    .Exclude("userPlaylists.contents"));

            List<BsonDocument> result = await query.ToListAsync();

            return result;
        }

        public async Task<BsonDocument> GetUserBasicDetails(string userId)
        {
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<UserCollection>.Projection.Exclude("password").Exclude("userPlaylists");

            var result = await _usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
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

        public async Task<UserCollection?> GetUserByUsername(string username) =>
            await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
        
        
        public async Task<UserCollection?> GetUserByEmail(string email) =>
            await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();


        public async Task<BsonDocument> GetUserProfilePhoto(string userId)
        {
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<UserCollection>.Projection.Include("profilePhotoUrl");

            var result =await _usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            return result;
        }
            

        public async Task UpdateUserProfilePhoto(string userId, string photoPath)
        {
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userId);
            var update = Builders<UserCollection>.Update.Set("profilePhotoUrl", photoPath);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UpdatePassword(string userId, string newPassword)
        {
            var filter = Builders<UserCollection>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserCollection>.Update.Set("password", newPassword);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UpdateEmail(string userId, string newEmail)
        {
            var filter = Builders<UserCollection>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserCollection>.Update.Set("email", newEmail);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UpdateName(string userId, string newName)
        {
            var filter = Builders<UserCollection>.Filter.Eq(p =>p.Id, userId);
            var updated = Builders<UserCollection>.Update.Set("name", newName);

            await _usersCollection.UpdateOneAsync(filter, updated);
        }
        
        public async Task UpdateUsername(string userId, string newUsername)
        {
            var filter = Builders<UserCollection>.Filter.Eq(p =>p.Id, userId);
            var updated = Builders<UserCollection>.Update.Set("username", newUsername);

            await _usersCollection.UpdateOneAsync(filter, updated);
        }
        
        public async Task DeleteUserPlaylist(string userId, ObjectId playlist)
        {
            var filter = Builders<UserCollection>.Filter.Eq(u=>u.Id, userId);
            var update = Builders<UserCollection>.Update.Pull("userPlaylists", playlist);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
    }
}