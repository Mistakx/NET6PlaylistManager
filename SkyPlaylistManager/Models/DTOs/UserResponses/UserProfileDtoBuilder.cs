using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDtoBuilder
{
    private UserProfileDto _userProfileDto;

    public UserProfileDtoBuilder BeginBuilding(UserDocument user, int playlistsWeeklyViewsAmount,
        int playlistsTotalViewsAmount, int playlistsContentAmount, int followersAmount,
        UserRecommendationsDocument? userViews)
    {
        _userProfileDto =
            new UserProfileDto(user, playlistsWeeklyViewsAmount, playlistsTotalViewsAmount, playlistsContentAmount,
                followersAmount, userViews);
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

    public UserProfileDto Build()
    {
        return _userProfileDto;
    }
}