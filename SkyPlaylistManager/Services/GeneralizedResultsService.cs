using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.Database.GenericResults;


namespace SkyPlaylistManager.Services
{
    public class GeneralizedResultsService
    {
        private readonly IMongoCollection<GenericResult> _generalizedResultsCollection;


        public GeneralizedResultsService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _generalizedResultsCollection = mongoDatabase.GetCollection<GenericResult>(("MultimediaContents"));
        }


        public async Task CreateGeneralizedResult(GenericResult genericResult) =>
            await _generalizedResultsCollection.InsertOneAsync(genericResult);

        public async Task UpdateGeneralizedResultUsage(string multimediaContentId, int increment)
        {
            var filter = Builders<GenericResult>.Filter.Eq(m => m.Id, multimediaContentId);
            // var update = Builders<GenericResult>.Update.Inc(m => m.Usages, increment);
            // await _multimediaContentsCollection.FindOneAndUpdateAsync(filter, update);
        }
    }
}
