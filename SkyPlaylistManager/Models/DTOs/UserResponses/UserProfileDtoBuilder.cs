using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDtoBuilder
{
    private UserProfileDto _userProfileDto;

    public UserProfileDtoBuilder BeginBuilding(UserDocument user, int viewablePlaylistsAmount)
    {
        _userProfileDto =
            new UserProfileDto(user, viewablePlaylistsAmount);
        return this;
    }
    
    public UserProfileDtoBuilder BeginBuilding(UserDocument user)
    {
        _userProfileDto =
            new UserProfileDto(user);
        return this;
    }

    public UserProfileDtoBuilder AddEmail(string email)
    {
        _userProfileDto.Email = email;
        return this;
    }

    public UserProfileDtoBuilder AddFollowed(bool followed)
    {
        _userProfileDto.Followed = followed;
        return this;
    }

    public UserProfileDtoBuilder AddWeeklyViewsAmount(UserRecommendationsDocument? userViews)
    {
        if (userViews != null)
        {
            _userProfileDto.WeeklyViewsAmount = userViews.WeeklyViewDates.Count;
        }
        else
        {
            _userProfileDto.WeeklyViewsAmount = 0;
        }

        return this;
    }

    public UserProfileDtoBuilder AddTotalViewsAmount(UserRecommendationsDocument? userViews)
    {
        if (userViews != null)
        {
            _userProfileDto.TotalViewsAmount = userViews.TotalViewsAmount;
        }
        else
        {
            _userProfileDto.TotalViewsAmount = 0;
        }

        return this;
    }

    public UserProfileDtoBuilder AddPlaylistsTotalViewsAmount(int playlistsTotalViewsAmount)
    {
        _userProfileDto.PlaylistsTotalViewsAmount = playlistsTotalViewsAmount;
        return this;
    }

    public UserProfileDtoBuilder AddPlaylistsWeeklyViewsAmount(int playlistsWeeklyViewsAmount)
    {
        _userProfileDto.PlaylistsWeeklyViewsAmount = playlistsWeeklyViewsAmount;
        return this;
    }

    public UserProfileDtoBuilder AddPlaylistsContentAmount(int playlistsContentAmount)
    {
        _userProfileDto.PlaylistsContentAmount = playlistsContentAmount;
        return this;
    }

    public UserProfileDtoBuilder AddFollowersAmount(int followersAmount)
    {
        _userProfileDto.FollowersAmount = followersAmount;
        return this;
    }
    
    public UserProfileDtoBuilder AddFollowingUsersAmount(int followingUsersAmount)
    {
        _userProfileDto.FollowingUsersAmount = followingUsersAmount;
        return this;
    }
    
    public UserProfileDtoBuilder AddFollowingPlaylistsAmount(int followingPlaylistsAmount)
    {
        _userProfileDto.FollowingPlaylistsAmount = followingPlaylistsAmount;
        return this;
    }

    public UserProfileDto Build()
    {
        return _userProfileDto;
    }
    
    
}