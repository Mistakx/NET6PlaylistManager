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

        // CREATE
        
        public async Task CreatePlaylist(PlaylistDocument newPlaylist, string userId)
        {
            await _playlistsCollection.InsertOneAsync(newPlaylist);

            var filter = Builders<UserDocument>.Filter.Eq(p => p.Id, userId);
            var update = Builders<UserDocument>.Update.Push("playlistIds", new ObjectId(newPlaylist.Id));
            await _usersCollection.UpdateOneAsync(filter, update);
        }

        
        // READ
        
        public async Task<PlaylistDocument?> GetPlaylistById(string playlistId) =>
            await _playlistsCollection.Find(p => p.Id == playlistId).FirstOrDefaultAsync();
        
        public async Task<BsonDocument> GetPlaylistContentOrderedIds(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var projection = Builders<PlaylistDocument>.Projection
                .Include("resultIds")
                .Exclude("_id");

            var result = await _playlistsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }

        public async Task<BsonDocument> GetPlaylistContent(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var query = _playlistsCollection.Aggregate().Match(filter)
                .Lookup(_generalizedResultsCollectionName, "resultIds", "_id", "content");

            var result = await query.FirstOrDefaultAsync();
            return result;
        }

        public async Task<bool> ContentIsAlreadyInPlaylist(string playlistId, string title,
            string playerFactoryName, string platformPlayerUrl)
        {
            var playlistGeneralizedResults = await GetPlaylistContent(playlistId);
            var deserializedPlaylistContents =
                BsonSerializer.Deserialize<GetPlaylistContentLookupDto>(playlistGeneralizedResults);

            foreach (var generalizedResult in deserializedPlaylistContents.Content)
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
        
        public async Task<int> GetTotalContentInPlaylists(List<ObjectId> playlistIds)
        {
            int totalItems = 0;

            foreach (var playlistId in playlistIds)
            {
                var playlist = await GetPlaylistById(playlistId.ToString());
                if (playlist != null)
                {
                    totalItems += playlist.ResultIds.Count;
                }
            }

            return totalItems;
        }


        // UPDATE
        
        public async Task UpdatePlaylistInformation(EditPlaylistDto updatedPlaylist)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, updatedPlaylist.PlaylistId);
            var updated = Builders<PlaylistDocument>.Update.Set(p => p.Title, updatedPlaylist.Title)
                .Set(p => p.Description, updatedPlaylist.Description)
                .Set(p => p.Visibility, updatedPlaylist.Visibility);

            await _playlistsCollection.UpdateOneAsync(filter, updated);
        }

        public async Task InsertContentInSpecificPlaylistPosition(SortContentDto newSortContent)
        {
            var resultIds = new List<ObjectId>(); // The method "PushEach" only works with lists
            var generalizedResultId = ObjectId.Parse(newSortContent.GeneralizedResultDatabaseId);
            resultIds.Add(generalizedResultId);

            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, newSortContent.PlaylistId);
            var update =
                Builders<PlaylistDocument>.Update.PushEach("resultIds", resultIds,
                    position: newSortContent.NewIndex);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertContentIdInPlaylist(string playlistId, ObjectId generalizedResultId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Push("resultIds", generalizedResultId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task UpdatePlaylistPhoto(string playlistId, string photoPath)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(u => u.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Set("thumbnailUrl", photoPath);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        
        // DELETE
        
        public async Task DeletePlaylist(string playlistId)
        {
            var deleteFilter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);

            await _playlistsCollection.DeleteOneAsync(deleteFilter);
        }

        public async Task DeleteContentIdFromPlaylist(string playlistId, ObjectId generalizedResultId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Pull("resultIds", generalizedResultId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }


 












 

    }
}