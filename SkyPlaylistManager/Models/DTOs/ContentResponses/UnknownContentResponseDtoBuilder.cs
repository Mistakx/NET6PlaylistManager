using SkyPlaylistManager.Models.Database;

namespace SkyPlaylistManager.Models.DTOs.ContentResponses;

public class UnknownContentResponseDtoBuilder
{
    private UnknownContentResponseDto _unknownContentResponseDto;

    public UnknownContentResponseDtoBuilder BeginBuilding(UnknownContentDocumentDto unknownGeneralizedResultDto)
    {
        _unknownContentResponseDto = new UnknownContentResponseDto(unknownGeneralizedResultDto);
        return this;
    }

    public UnknownContentResponseDtoBuilder AddViews(ContentRecommendationsDocument? contentViews)
    {
        if (contentViews != null)
        {
            _unknownContentResponseDto.TotalViewsAmount = contentViews.TotalViewsAmount;
            _unknownContentResponseDto.WeeklyViewsAmount = contentViews.WeeklyViewDates.Count;
        }
        else
        {
            _unknownContentResponseDto.TotalViewsAmount = 0;
            _unknownContentResponseDto.WeeklyViewsAmount = 0;
        }

        return this;
    }

    public UnknownContentResponseDto Build()
    {
        return _unknownContentResponseDto;
    }
}