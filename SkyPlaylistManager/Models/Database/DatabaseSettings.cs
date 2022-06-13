namespace SkyPlaylistManager.Models.Database
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string GeneralizedResultsCollectionName { get; set; }
        public string PlaylistsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string UserRecommendationsCollectionName { get; set; }
        public string PlaylistRecommendationsCollectionName { get; set; }
        public string ContentRecommendationsCollectionName { get; set; }
    }
}