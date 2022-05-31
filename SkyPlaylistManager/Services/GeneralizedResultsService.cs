using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.GeneralizedResults;


namespace SkyPlaylistManager.Services
{
    public class GeneralizedResultsService
    {
        private readonly IMongoCollection<GeneralizedResult> _generalizedResultsCollection;

        public GeneralizedResultsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _generalizedResultsCollection = mongoDatabase.GetCollection<GeneralizedResult>(databaseSettings.Value.GeneralizedResultsCollectionName);
        }

        public async Task CreateGeneralizedResult(GeneralizedResult generalizedResult) =>
            await _generalizedResultsCollection.InsertOneAsync(generalizedResult);
    }
}