using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDto
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public int WeeklyViewsAmount { get; set; }
    public int TotalViewsAmount { get; set; }
    public string? Email { get; set; }
    public bool? Followed { get; set; }

    public UserProfileDto(string name, string username, string profilePhotoUrl, UserRecommendationsDocument? userViews)
    {
        Name = name;
        Username = username;
        ProfilePhotoUrl = profilePhotoUrl;

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
    
}