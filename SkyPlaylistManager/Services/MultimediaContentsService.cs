using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;


namespace SkyPlaylistManager.Services
{
    public class MultimediaContentsService
    {
        private readonly IMongoCollection<MultimediaContent> _multimediaContentsCollection;


        public MultimediaContentsService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _multimediaContentsCollection = mongoDatabase.GetCollection<MultimediaContent>(("MultimediaContents"));
        }


        public async Task CreateMultimediaContent(MultimediaContent newMultimediaContent) =>
            await _multimediaContentsCollection.InsertOneAsync(newMultimediaContent);

        public async Task UpdateMultimediaContentUsage(string multimediaContentId, int increment)
        {
            var filter = Builders<MultimediaContent>.Filter.Eq(m => m.Id, multimediaContentId);
            var update = Builders<MultimediaContent>.Update.Inc(m => m.Usages, increment);

            await _multimediaContentsCollection.FindOneAndUpdateAsync(filter, update);
        }
    }
}
