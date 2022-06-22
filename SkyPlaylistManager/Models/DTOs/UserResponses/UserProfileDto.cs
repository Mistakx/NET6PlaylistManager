using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDto
{
    public string Username { get; }
    public string? Name { get; }
    public string? Email { get; set; }
    public string ProfilePhotoUrl { get; }

    public int? ViewablePlaylistsAmount { get; }
    public int? PlaylistsWeeklyViewsAmount { get; set; }
    public int? PlaylistsTotalViewsAmount { get; set; }
    public int? PlaylistsContentAmount { get; set; }

    public int? WeeklyViewsAmount { get; set; }
    public int? TotalViewsAmount { get; set; }

    public int? FollowersAmount { get; set; }
    public int? FollowingUsersAmount { get; set; }
    public int? FollowingPlaylistsAmount { get; set; }

    public bool? Followed { get; set; }


    public UserProfileDto(UserDocument user, int viewablePlaylistsAmount)
    {
        Name = user.Name;
        Username = user.Username;
        ProfilePhotoUrl = user.ProfilePhotoUrl;

        ViewablePlaylistsAmount = viewablePlaylistsAmount;
    }

    public UserProfileDto(UserDocument user)
    {
        Username = user.Username;
        ProfilePhotoUrl = user.ProfilePhotoUrl;
    }
}