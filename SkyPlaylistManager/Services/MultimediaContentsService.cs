using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.Database.GenericResults;


namespace SkyPlaylistManager.Services
{
    public class MultimediaContentsService
    {
        private readonly IMongoCollection<GenericResult> _multimediaContentsCollection;


        public MultimediaContentsService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _multimediaContentsCollection = mongoDatabase.GetCollection<GenericResult>(("MultimediaContents"));
        }


        public async Task CreateMultimediaContent(GenericResult genericResult) =>
            await _multimediaContentsCollection.InsertOneAsync(genericResult);

        public async Task UpdateMultimediaContentUsage(string multimediaContentId, int increment)
        {
            var filter = Builders<GenericResult>.Filter.Eq(m => m.Id, multimediaContentId);
            // var update = Builders<GenericResult>.Update.Inc(m => m.Usages, increment);
            // await _multimediaContentsCollection.FindOneAndUpdateAsync(filter, update);
        }
    }
}
