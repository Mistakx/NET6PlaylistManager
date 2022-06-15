﻿namespace SkyPlaylistManager.Models.DTOs.RecommendationRequests;

public class GetTrendingPlaylistsDto
{
    public string PlaylistName { get; set; }
    public int Limit { get; set; }
    public int PageNumber {get; set;}
    public string SessionToken { get; set; }
}