using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;


namespace SkyPlaylistManager.Services
{
    public class CommunityService
    {
        private readonly IMongoCollection<UserRecommendationsDocument> _recommendationsCollection;
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
        }
        
        public async Task<IEnumerable<UserDocument>> GetUsers(string username)
        {

            var results = new List<UserDocument>();

            var filter = Builders<UserDocument>.Filter.Regex("name", new BsonRegularExpression("(?i)^" + username));
            var result = await _userCollection.Find(filter).ToListAsync();
            foreach (var user in result)
            {
                results.Add(user);
            }
            
            filter = Builders<UserDocument>.Filter.Regex("username", new BsonRegularExpression("(?i)^" + username));
            result = await _userCollection.Find(filter).ToListAsync();
            foreach (var user in result)
            {
                results.Add(user);
            }

            return result.Distinct().ToArray();

        }
    }
}