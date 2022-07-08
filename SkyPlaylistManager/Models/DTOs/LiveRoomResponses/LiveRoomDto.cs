using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Models.DTOs.LiveRoomResponses;

public class LiveRoomUserDto
{
    public UserProfileDto User { get; set; }
    public JsonObject? CurrentlyPlaying { get; set; }

    public LiveRoomUserDto(UserDocument userDocument, JsonObject? content)
    {
        UserProfileDtoBuilder userProfileDtoBuilder = new UserProfileDtoBuilder();
        User = userProfileDtoBuilder.BeginBuilding(userDocument).Build();
        CurrentlyPlaying = content;
    }
}