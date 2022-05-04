using SkyPlaylistManager.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Services
{
    public class PlaylistsService
    {
        private readonly IMongoCollection<PlaylistCollection> _playListsCollection;


        public PlaylistsService(IOptions<DatabaseSettings> SkyPlaylistManagerDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                SkyPlaylistManagerDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                SkyPlaylistManagerDatabaseSettings.Value.DatabaseName);

            _playListsCollection = mongoDatabase.GetCollection<PlaylistCollection>(("Playlists"));
        }





        public async Task<List<BsonDocument>> GetPlaylistContents(string playlistId)
        {
            
            var filter = Builders<PlaylistCollection>.Filter.Eq(p => p.Id, playlistId);
            var query = _playListsCollection.Aggregate().Match(filter)
                .Lookup("MultimediaContents", "contents", "_id", "contents");
               // .Unwind("owner");

            List <BsonDocument> result = await query.ToListAsync();

            return result;
        }



        public async Task<List<PlaylistCollection>> GetAllPlaylists() =>
            await _playListsCollection.Find(_ => true).ToListAsync();


        public async Task CreatePlaylist(PlaylistCollection newPlaylist) =>
            await _playListsCollection.InsertOneAsync(newPlaylist);


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



    }
}
