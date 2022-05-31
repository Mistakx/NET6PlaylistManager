using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.PlaylistRequests;

namespace SkyPlaylistManager.Services
{
    public class PlaylistsService
    {
        private readonly IMongoCollection<PlaylistDocument> _playlistsCollection;
        private readonly IMongoCollection<UserDocument> _usersCollection;
        private readonly IMongoCollection<GeneralizedResultsService> _generalizedResultsCollection;
        private readonly string _generalizedResultsCollectionName;


        public PlaylistsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _playlistsCollection =
                mongoDatabase.GetCollection<PlaylistDocument>(databaseSettings.Value.PlaylistsCollectionName);
            _usersCollection =
                mongoDatabase.GetCollection<UserDocument>(databaseSettings.Value.UsersCollectionName);

            _generalizedResultsCollectionName = databaseSettings.Value.GeneralizedResultsCollectionName;
            // _generalizedResultsCollection =
            //     mongoDatabase.GetCollection<GeneralizedResultDocument>(DatabaseCollectionNames.GetGeneralizedResultsCollectionName());
            //
        }


        public async Task InsertMultimediaContentInSpecificPosition(SortContentsDto newSortContents)
        {
            var contentsList = new List<ObjectId>(); // The method "PushEach" only works with lists
            ObjectId multimediaContentId = ObjectId.Parse(newSortContents.GeneralizedResultId);
            contentsList.Add(multimediaContentId);

            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, newSortContents.PlaylistId);
            var update =
                Builders<PlaylistDocument>.Update.PushEach("contents", contentsList,
                    position: newSortContents.NewIndex);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }


        public async Task<List<BsonDocument>> GetPlaylistsByOwner(string userId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Owner, userId);
            var projection = Builders<PlaylistDocument>.Projection.Exclude("owner").Exclude("sharedWith")
                .Exclude("contents");

            var result = await _playlistsCollection.Find(filter).Project(projection).ToListAsync();
            return result;
        }

        public async Task<BsonDocument> GetPlaylistDetails(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var projection = Builders<PlaylistDocument>.Projection.Exclude("contents").Exclude("owner")
                .Exclude("sharedWith");

            var result = await _playlistsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return result;
        }

        public async Task<BsonDocument> GetPlaylistGeneralizedResults(string playlistId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var query = _playlistsCollection.Aggregate().Match(filter)
                .Lookup(_generalizedResultsCollectionName, "contents", "_id", "contents")
                .Project(Builders<BsonDocument>.Projection.Include("contents").Exclude("_id"));

            var result = await query.FirstOrDefaultAsync();

            return result;
        }

        // public async Task<bool> GeneralizedResultAlreadyInPlaylist(string playlistId, string title,
        //     string playerFactoryName, string platformPlayerUrl)
        // {
        //     var playlistGeneralizedResults = await _generalizedResultsCollection
        //         .Find(playlist => playlist. == playlistId).FirstOrDefaultAsync();
        //     foreach (var playlist in playlistGeneralizedResults.Contents!)
        //     {
        //         if (playlist.)
        //         {
        //             return true;
        //         }
        //     }
        //
        //
        //     return false;
        // }

        public async Task<UserDocument?> GetG(string username) =>
            await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

        public async Task<List<PlaylistDocument>> GetAllPlaylists() =>
            await _playlistsCollection.Find(_ => true).ToListAsync();


        public async Task CreatePlaylist(PlaylistDocument newPlaylist) =>
            await _playlistsCollection.InsertOneAsync(newPlaylist);

        public async Task InsertUserPlaylist(string userId, ObjectId playlistId)
        {
            var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);
            var update = Builders<UserDocument>.Update.Push("userPlaylists", playlistId);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertUserInSharedWithArray(string playlistId, ObjectId userId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Push("sharedWith", userId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertGeneralizedResultInPlaylist(string playlistId, ObjectId generalizedResultId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Push("contents", generalizedResultId);

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

        public async Task DeleteShare(string playlistId, ObjectId userId)
        {
            var deleteFilter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Pull("sharedWith", userId);

            await _playlistsCollection.UpdateOneAsync(deleteFilter, update);
        }

        public async Task DeleteMultimediaContentInPlaylist(string playlistId, ObjectId generalizedResultId)
        {
            var filter = Builders<PlaylistDocument>.Filter.Eq(p => p.Id, playlistId);
            var update = Builders<PlaylistDocument>.Update.Pull("contents", generalizedResultId);

            await _playlistsCollection.UpdateOneAsync(filter, update);
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