using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;

namespace SkyPlaylistManager.Services
{
    public class PlaylistsService
    {
        private readonly IMongoCollection<PlaylistDocument> _playlistsCollection;
        private readonly IMongoCollection<UserDocument> _usersCollection;
        private readonly string _generalizedResultsCollectionName;


        public PlaylistsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            var playlistsCollectionName = databaseSettings.Value.PlaylistsCollectionName;
            _playlistsCollection =
                mongoDatabase.GetCollection<PlaylistDocument>(playlistsCollectionName);

            _usersCollection =
                mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value.UsersCollectionName);

            _generalizedResultsCollectionName = databaseSettings.Value.GeneralizedResultsCollectionName;
        }

        public async Task InsertGeneralizedResultInSpecificPosition(SortPlaylistResultsDto newSortPlaylistResults,
            int currentAmountOfResults)
        {
            var resultIds = new List<ObjectId>(); // The method "PushEach" only works with lists
            var generalizedResultId = ObjectId.Parse(newSortPlaylistResults.GeneralizedResultDatabaseId);
            resultIds.Add(generalizedResultId);

            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, newSortPlaylistResults.PlaylistId);
            var update =
                Builders<PlaylistDocument>.Update.PushEach("resultIds", resultIds,
                    position: newSortPlaylistResults.NewIndex).Set("resultsAmount", currentAmountOfResults + 1);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertPlaylistInSpecificPosition(string playlistId, int newIndex, string userId)
        {
            var playlistIds = new List<ObjectId>(); // The method "PushEach" only works with lists
            var generalizedResultId = ObjectId.Parse(playlistId);
            playlistIds.Add(generalizedResultId);

            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update =
                Builders<UserDocument>.Update.PushEach("playlistIds", playlistIds,
                    position: newIndex);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task<BsonDocument> GetPlaylistDetails(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var projection = Builders<PlaylistDocument>.Projection
                .Exclude("resultIds")
                .Exclude("owner");

            var result = await _playlistsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }

        public async Task<BsonDocument> GetPlaylistContentOrderedIds(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var projection = Builders<PlaylistDocument>.Projection
                .Include("resultIds")
                .Exclude("_id");

            var result = await _playlistsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }

        public async Task<BsonDocument> GetPlaylistGeneralizedResults(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var query = _playlistsCollection.Aggregate().Match(filter)
                .Lookup(_generalizedResultsCollectionName, "resultIds", "_id", "results")
                .Project(Builders<BsonDocument>.Projection.Include("results").Exclude("_id"));

            var result = await query.FirstOrDefaultAsync();
            return result;
        }

        public async Task<bool> GeneralizedResultAlreadyInPlaylist(string playlistId, string title,
            string playerFactoryName, string platformPlayerUrl)
        {
            var playlistGeneralizedResults = await GetPlaylistGeneralizedResults(playlistId);
            var deserializedPlaylistContents =
                BsonSerializer.Deserialize<PlaylistResultsDto>(playlistGeneralizedResults);

            foreach (var generalizedResult in deserializedPlaylistContents.Results)
            {
                if (generalizedResult.Title == title &&
                    generalizedResult.PlayerFactoryName == playerFactoryName &&
                    generalizedResult.PlatformPlayerUrl == platformPlayerUrl)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task CreatePlaylist(PlaylistDocument newPlaylist, string userId)
        {
            await _playlistsCollection.InsertOneAsync(newPlaylist);

            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserDocument>.Update.Push("playlistIds", new ObjectId(newPlaylist.Id));
            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertGeneralizedResultInPlaylist(string playlistId, ObjectId generalizedResultId,
            int currentResultsAmount)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Push("resultIds", generalizedResultId)
                .Set("resultsAmount", currentResultsAmount + 1);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task<PlaylistDocument?> GetPlaylistById(string playlistId) =>
            await _playlistsCollection.Find(p => p.Id == playlistId).FirstOrDefaultAsync();

        public async Task UpdatePlaylist(EditPlaylistDto updatedPlaylist)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, updatedPlaylist.PlaylistId);
            var updated = Builders<PlaylistDocument>.Update.Set(p => p.Title, updatedPlaylist.Title)
                .Set(p => p.Description, updatedPlaylist.Description)
                .Set(p => p.Visibility, updatedPlaylist.Visibility);

            await _playlistsCollection.UpdateOneAsync(filter, updated);
        }

        public async Task DeletePlaylist(string playlistId)
        {
            var deleteFilter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);

            await _playlistsCollection.DeleteOneAsync(deleteFilter);
        }

        public async Task DeleteMultimediaContentInPlaylist(string playlistId, ObjectId generalizedResultId,
            int currentResultsAmount)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Pull("resultIds", generalizedResultId)
                .Set("resultsAmount", currentResultsAmount - 1);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task DeletePlaylistInUser(string userId, ObjectId playlistId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update  = Builders<UserDocument>.Update.Pull("playlistIds", playlistId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdatePlaylistPhoto(string playlistId, string photoPath)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(u => u.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Set("thumbnailUrl", photoPath);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task<BsonDocument> GetPlaylistPhoto(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(u => u.Id, playlistId);
            var projection = Builders<PlaylistDocument>.Projection.Include("thumbnailUrl");

            var result = await _playlistsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            return result;
        }
    }
}