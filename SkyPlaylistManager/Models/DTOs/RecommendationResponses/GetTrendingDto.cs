﻿using MongoDB.Bson.Serialization.Attributes;
using SkyPlaylistManager.Models.DTOs.GeneralizedResults;

namespace SkyPlaylistManager.Models.DTOs.RecommendationResponses;

public class GetTrendingDto
{
    [BsonElement("generalizedResult")] public UnknownGeneralizedResultDto generalizedResult { get; set; }
}