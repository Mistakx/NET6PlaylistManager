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

    public UnknownContentResponseDtoBuilder AddMonthlyViews(ContentRecommendationsDocument? contentViews)
    {
        if (contentViews != null)
        {
            _unknownContentResponseDto.TotalViewsAmount = contentViews.TotalViewsAmount;
            _unknownContentResponseDto.MonthlyViewsAmount = contentViews.MonthlyViewDates.Count;
        }
        else
        {
            _unknownContentResponseDto.TotalViewsAmount = 0;
            _unknownContentResponseDto.MonthlyViewsAmount = 0;
        }

        return this;
    }
    
    public UnknownContentResponseDtoBuilder AddWeeklyViews(ContentRecommendationsDocument? contentViews)
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
    
    public UnknownContentResponseDtoBuilder AddDailyViews(ContentRecommendationsDocument? contentViews)
    {
        if (contentViews != null)
        {
            _unknownContentResponseDto.TotalViewsAmount = contentViews.TotalViewsAmount;
            _unknownContentResponseDto.DailyViewsAmount = contentViews.DailyViewDates.Count;
        }
        else
        {
            _unknownContentResponseDto.TotalViewsAmount = 0;
            _unknownContentResponseDto.DailyViewsAmount = 0;
        }

        return this;
    }

    public UnknownContentResponseDto Build()
    {
        return _unknownContentResponseDto;
    }
}