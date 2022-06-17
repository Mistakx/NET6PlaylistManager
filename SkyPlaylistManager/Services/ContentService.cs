using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;

namespace SkyPlaylistManager.Services
{
    public class ContentService
    {
        private readonly IMongoCollection<UnknownContentDocumentDto> _generalizedResultsCollection;

        public ContentService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _generalizedResultsCollection = mongoDatabase.GetCollection<UnknownContentDocumentDto>(databaseSettings.Value.GeneralizedResultsCollectionName);
        }

        public async Task CreateContent(UnknownContentDocumentDto generalizedResult) =>
            await _generalizedResultsCollection.InsertOneAsync(generalizedResult);
        
        
    }
}