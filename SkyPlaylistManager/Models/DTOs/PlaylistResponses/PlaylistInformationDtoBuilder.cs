using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationDtoBuilder
{
    private PlaylistInformationDto playlistInformationDto;

    public PlaylistInformationDtoBuilder BeginBuilding(string playlistId, string title, string description,
        string thumbnailUrl, int resultsAmount)
    {
        playlistInformationDto =
            new PlaylistInformationDto(playlistId, title, description, thumbnailUrl, resultsAmount);
        return this;
    }

    public PlaylistInformationDtoBuilder AddViews(PlaylistRecommendationsDocument? playlistViews)
    {
        if (playlistViews != null)
        {
            playlistInformationDto.TotalViewsAmount = playlistViews.TotalViewsAmount;
            playlistInformationDto.WeeklyViewsAmount = playlistViews.WeeklyViewsAmount;
        }
        else
        {
            playlistInformationDto.TotalViewsAmount = 0;
            playlistInformationDto.WeeklyViewsAmount = 0;
        }

        return this;
    }

    public PlaylistInformationDtoBuilder AddFollowing(bool userAlreadyFollowingPlaylist)
    {
        playlistInformationDto.Followed = userAlreadyFollowingPlaylist;
        return this;
    }

    public PlaylistInformationDtoBuilder AddVisibility(string playlistVisibility)
    {
        playlistInformationDto.Visibility = playlistVisibility;
        return this;
    }

    public PlaylistInformationDto Build()
    {
        return playlistInformationDto;
    }
}