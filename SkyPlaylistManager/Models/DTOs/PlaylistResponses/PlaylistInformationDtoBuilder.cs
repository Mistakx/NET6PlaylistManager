using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.PlaylistResponses;

public class PlaylistInformationDtoBuilder
{
    private PlaylistInformationDto _playlistInformationDto;

    public PlaylistInformationDtoBuilder BeginBuilding(string playlistId, string title, string description,
        string thumbnailUrl, int resultsAmount)
    {
        _playlistInformationDto =
            new PlaylistInformationDto(playlistId, title, description, thumbnailUrl, resultsAmount);
        return this;
    }

    public PlaylistInformationDtoBuilder AddViews(PlaylistRecommendationsDocument? playlistViews)
    {
        if (playlistViews != null)
        {
            _playlistInformationDto.TotalViewsAmount = playlistViews.TotalViewsAmount;
            _playlistInformationDto.WeeklyViewsAmount = playlistViews.WeeklyViewDates.Count;
        }
        else
        {
            _playlistInformationDto.TotalViewsAmount = 0;
            _playlistInformationDto.WeeklyViewsAmount = 0;
        }

        return this;
    }

    public PlaylistInformationDtoBuilder AddFollowing(bool userAlreadyFollowingPlaylist)
    {
        _playlistInformationDto.Followed = userAlreadyFollowingPlaylist;
        return this;
    }

    public PlaylistInformationDtoBuilder AddVisibility(string playlistVisibility)
    {
        _playlistInformationDto.Visibility = playlistVisibility;
        return this;
    }

    public PlaylistInformationDto Build()
    {
        return _playlistInformationDto;
    }
}