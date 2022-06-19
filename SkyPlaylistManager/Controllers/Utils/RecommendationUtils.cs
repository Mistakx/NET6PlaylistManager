using MongoDB.Bson;
using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Controllers.Utils;

public class RecommendationUtils
{
    
    public static bool PlaylistBelongsToRequestingUser(PlaylistDocument playlist, UserDocument user)
    {
        foreach (var userPlaylistId in user.UserPlaylistIds)
        {
            if (new ObjectId(playlist.Id) == userPlaylistId)
            {
                return true;
            }
        }

        return false;
    }
}