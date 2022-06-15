namespace SkyPlaylistManager.Models.DTOs.UserResponses;

public class UserProfileDto : IEqualityComparer<UserProfileDto>
{
    public string? Email { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string ProfilePhotoUrl { get; set; }

    public int WeeklyViewsAmount { get; set; }

    public int TotalViewsAmount { get; set; }

    public UserProfileDto(string email, string name, string username, string profilePhotoUrl,
        int weeklyViewsAmount, int totalViewsAmount)
    {
        Email = email;
        Name = name;
        Username = username;
        ProfilePhotoUrl = profilePhotoUrl;
        WeeklyViewsAmount = weeklyViewsAmount;
        TotalViewsAmount = totalViewsAmount;
    }

    public UserProfileDto(string name, string username, string profilePhotoUrl, int weeklyViewsAmount,
        int totalViewsAmount)
    {
        Name = name;
        Username = username;
        ProfilePhotoUrl = profilePhotoUrl;
        WeeklyViewsAmount = weeklyViewsAmount;
        TotalViewsAmount = totalViewsAmount;
    }

    public bool Equals(UserProfileDto x, UserProfileDto y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Email == y.Email && x.Name == y.Name && x.Username == y.Username && x.ProfilePhotoUrl == y.ProfilePhotoUrl && x.WeeklyViewsAmount == y.WeeklyViewsAmount && x.TotalViewsAmount == y.TotalViewsAmount;
    }

    public int GetHashCode(UserProfileDto obj)
    {
        return HashCode.Combine(obj.Email, obj.Name, obj.Username, obj.ProfilePhotoUrl, obj.WeeklyViewsAmount, obj.TotalViewsAmount);
    }
}