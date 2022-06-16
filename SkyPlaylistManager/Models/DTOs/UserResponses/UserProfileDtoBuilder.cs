using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDtoBuilder
{
    public string Name { get; }
    public string Username { get; }
    public string ProfilePhotoUrl { get; }
    public int WeeklyViewsAmount { get; }
    public int TotalViewsAmount { get; }
    public string? Email { get; set; }
    public bool? Followed { get; set; }

    private UserProfileDto _userProfileDto;

    public UserProfileDtoBuilder BeginBuilding(string name, string username, string profilePhotoUrl,
        UserRecommendationsDocument? userViews)
    {
        _userProfileDto = new UserProfileDto(name, username, profilePhotoUrl, userViews);
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