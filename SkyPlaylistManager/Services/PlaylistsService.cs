using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.Playlist;

namespace SkyPlaylistManager.Services
{
    public class PlaylistsService
    {
        private readonly IMongoCollection<PlaylistCollection> _playListsCollection;
        private readonly IMongoCollection<UserCollection> _usersCollection;


        public PlaylistsService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _playListsCollection = mongoDatabase.GetCollection<PlaylistCollection>(("Playlists"));
            _usersCollection = mongoDatabase.GetCollection<UserCollection>(("Users"));
        }





        public async Task InsertMultimediaContentInSpecificPosition(SortContentsDTO newSortContents)
        {
            var contentsList = new List<ObjectId>(); // The method "PushEach" only works with lists
            ObjectId multimediaContentId = ObjectId.Parse(newSortContents.MultimediaContentId);
            contentsList.Add(multimediaContentId);

            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, newSortContents.PlaylistId);
            var update = Builders<PlaylistCollection>.Update.PushEach("contents", contentsList, position: newSortContents.NewPosition);

            await _playListsCollection.UpdateOneAsync(filter, update);
        }



        public async Task<List<BsonDocument>> GetPlaylistsByOwner(string userId)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Owner, userId);
            var projection = Builders<PlaylistCollection>.Projection.Exclude("owner").Exclude("sharedWith").Exclude("contents");

            var result = await _playListsCollection.Find(filter).Project(projection).ToListAsync();
            return result;
        }

        public async Task<BsonDocument> GetPlaylistContents(string playlistId)
        {
            
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistId);
            var query = _playListsCollection.Aggregate().Match(filter)
                .Lookup("MultimediaContents", "contents", "_id", "contents")
                .Lookup("Users", "owner", "_id", "owner")
                // .Lookup("Users", "sharedWith", "_id", "sharedWith")

                .Project(Builders<BsonDocument>.Projection.Exclude("sharedWith"))
                .Project(Builders<BsonDocument>.Projection.Exclude("owner.userPlaylists").Exclude("owner.password"))
                .Project(Builders<BsonDocument>.Projection.Exclude("sharedWith.userPlaylists").Exclude("sharedWith.password"))
                .Unwind("owner");

            var result = await query.FirstOrDefaultAsync();

            return result;
        }
        
        public async Task<List<PlaylistCollection>> GetAllPlaylists() =>
            await _playListsCollection.Find(_ => true).ToListAsync();


        public async Task CreatePlaylist(PlaylistCollection newPlaylist) =>
            await _playListsCollection.InsertOneAsync(newPlaylist);

        public async Task InsertUserPlaylist(string userID, ObjectId playlistID)
        {
            var filter = Builders<UserCollection>.Filter.Eq(u => u.Id, userID);
            var update = Builders<UserCollection>.Update.Push("userPlaylists", playlistID);

            await _usersCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task InsertUserInSharedWithArray(string playlistID, ObjectId userID)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var update = Builders<PlaylistCollection>.Update.Push("sharedWith", userID);

            await _playListsCollection.UpdateOneAsync(filter, update);
        }

        public async Task InsertMultimediaContentInPlaylist(string playlistID, ObjectId MultimediaContentID)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var update = Builders<PlaylistCollection>.Update.Push("contents", MultimediaContentID);

            await _playListsCollection.UpdateOneAsync(filter, update);
        }
    
        public async Task<PlaylistCollection?> GetPlaylistById(string playlistId) =>
            await _playListsCollection.Find(p => p.Id == playlistId).FirstOrDefaultAsync();
        
        public async Task UpdateTitle(string playlistID, string newTitle)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var updated = Builders<PlaylistCollection>.Update.Set("title", newTitle);

            await _playListsCollection.UpdateOneAsync(filter, updated);
        }
        
        public async Task UpdateDescription(string playlistID, string newDescription)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var updated = Builders<PlaylistCollection>.Update.Set("description", newDescription);

            await _playListsCollection.UpdateOneAsync(filter, updated);
        }
        
        public async Task UpdateVisibility(string playlistID, string newVisibility)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id,playlistID);
            var updated = Builders<PlaylistCollection>.Update.Set("visibility", newVisibility);

            await _playListsCollection.UpdateOneAsync(filter, updated);
        }
        
        public async Task DeletePlaylist(string playlistID)
        {
            var deleteFilter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);

            await _playListsCollection.DeleteOneAsync(deleteFilter);
        }
        
        public async Task DeleteShare(string playlistID, ObjectId userID)
        {
            var deleteFilter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var update = Builders<PlaylistCollection>.Update.Pull("sharedWith", userID);

            await _playListsCollection.UpdateOneAsync(deleteFilter, update);

        }
        
        public async Task DeleteMultimediaContentInPlaylist(string playlistID, ObjectId MultimediaContentID)
        {
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistID);
            var update = Builders<PlaylistCollection>.Update.Pull("contents", MultimediaContentID);

            await _playListsCollection.UpdateOneAsync(filter, update);
        }


    }
}
