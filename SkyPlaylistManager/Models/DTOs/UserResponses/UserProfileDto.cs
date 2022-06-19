using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDto
{
    public string Username { get; }
    public string? Name { get; }
    public string ProfilePhotoUrl { get; }

    public int? WeeklyViewsAmount { get; }
    public int? TotalViewsAmount { get; }

    public int? PlaylistsAmount { get; }
    public int? PlaylistsWeeklyViewsAmount { get; }
    public int? PlaylistsTotalViewsAmount { get; }

    public int? FollowersAmount { get; }

    public string? Email { get; set; }
    public bool? Followed { get; set; }


    public UserProfileDto(UserDocument user, int playlistsWeeklyViewsAmount, int playlistsTotalViewsAmount,
        UserRecommendationsDocument? userViews)
    {
        Name = user.Name;
        Username = user.Username;
        ProfilePhotoUrl = user.ProfilePhotoUrl;

        PlaylistsAmount = user.UserPlaylistIds.Count;
        PlaylistsWeeklyViewsAmount = playlistsWeeklyViewsAmount;
        PlaylistsTotalViewsAmount = playlistsTotalViewsAmount;

        FollowersAmount = user.UsersFollowingIds.Count;

        if (userViews != null)
        {
            WeeklyViewsAmount = userViews.WeeklyViewDates.Count;
            TotalViewsAmount = userViews.TotalViewsAmount;
        }
        else
        {
            WeeklyViewsAmount = 0;
            TotalViewsAmount = 0;
        }
    }

    public UserProfileDto(UserDocument user)
    {
        Name = user.Name;
        ProfilePhotoUrl = user.ProfilePhotoUrl;
    }

}