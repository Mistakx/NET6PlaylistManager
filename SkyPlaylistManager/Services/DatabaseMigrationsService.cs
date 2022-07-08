using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.ContentResponses;

namespace SkyPlaylistManager.Services
{
    public class DatabaseMigrationsService
    {
        private readonly IMongoCollection<ContentRecommendationsDocument> _contentRecommendationsCollection;

        public DatabaseMigrationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);


            _contentRecommendationsCollection = mongoDatabase.GetCollection<ContentRecommendationsDocument>(databaseSettings
                .Value
                .ContentRecommendationsCollectionName);
        }


        // UPDATE


        public async Task UpdatePlatformDatabase()
        {
            var filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/youtube/");
            var update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "YouTube");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/twitch/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Twitch");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/twitch/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Twitch");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/mixcloud/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Mixcloud");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
                        
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/dailymotion/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "DailyMotion");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.ResultType, "/Spotify/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Spotify");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlayerFactoryName, "/Spotify/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Spotify");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
            filter = Builders<ContentRecommendationsDocument>.Filter.Regex(p => p.GeneralizedResult.PlatformPlayerUrl, "/soundcloud/");
            update = Builders<ContentRecommendationsDocument>.Update.Set("generalizedResult.platformName", "Soundcloud");
            await _contentRecommendationsCollection.UpdateManyAsync(filter, update);
            
        }
    }
}