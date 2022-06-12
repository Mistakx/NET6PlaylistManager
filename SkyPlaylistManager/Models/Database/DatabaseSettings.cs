namespace SkyPlaylistManager.Models.Database
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string GeneralizedResultsCollectionName { get; set; }
        public string PlaylistsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string RecommendationsCollectionName { get; set; }
    }
}