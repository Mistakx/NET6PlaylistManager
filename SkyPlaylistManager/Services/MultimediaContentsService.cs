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
    }
}
