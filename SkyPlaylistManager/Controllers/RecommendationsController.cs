﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SkyPlaylistManager.Models.Database;
using SkyPlaylistManager.Models.DTOs.ContentResponses;
using SkyPlaylistManager.Services;
using SkyPlaylistManager.Models.DTOs.PlaylistResponses;
using SkyPlaylistManager.Models.DTOs.RecommendationRequests;
using SkyPlaylistManager.Models.DTOs.RecommendationResponses;
using SkyPlaylistManager.Models.DTOs.UserResponses;

namespace SkyPlaylistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendationsController : Controller
    {
        private readonly ContentRecommendationsService _contentRecommendationsService;
        private readonly PlaylistRecommendationsService _playlistRecommendationsService;
        private readonly UserRecommendationsService _userRecommendationsService;
        private readonly UsersService _userService;
        private readonly PlaylistsService _playlistsService;
        private readonly CommunityService _communityService;
        private readonly SessionTokensService _sessionTokensService;

        public RecommendationsController(
            ContentRecommendationsService contentRecommendationsService,
            PlaylistRecommendationsService playlistRecommendationsService,
            UserRecommendationsService userRecommendationsService,
            UsersService usersService,
            PlaylistsService playlistsService,
            CommunityService communityService,
            SessionTokensService sessionTokensService)
        {
            _contentRecommendationsService = contentRecommendationsService;
            _playlistRecommendationsService = playlistRecommendationsService;
            _userRecommendationsService = userRecommendationsService;
            _userService = usersService;
            _playlistsService = playlistsService;
            _communityService = communityService;
            _sessionTokensService = sessionTokensService;
        }


        //! Content
        [HttpPost("saveContentView")]
        public async Task<IActionResult> SaveContentView(SaveContentViewDto request)
        {
            try
            {
                var recommendation = await _contentRecommendationsService.GetResultInRecommended(
                    request.Content.Title,
                    request.Content.PlayerFactoryName, request.Content.PlatformPlayerUrl!);

                if (recommendation == null)
                {
                    await _contentRecommendationsService.SaveView(request);
                }
                else
                {
                    await _contentRecommendationsService.AddViewToResult(request.Content.Title,
                        request.Content.PlayerFactoryName, request.Content.PlatformPlayerUrl!,
                        recommendation.WeeklyViewDates.Count, recommendation.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingContent")]
        public async Task<List<UnknownContentResponseDto>?> GetTrendingContent(GetTrendingContentDto request)
        {
            try
            {
                var unknownContentResponseDtoBuilder = new UnknownContentResponseDtoBuilder();
                _contentRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingResults = await _contentRecommendationsService.GetTrendingContent(request.Limit);
                if (trendingResults == null) return new List<UnknownContentResponseDto>();

                var deserializedList = new List<UnknownContentResponseDto>();
                foreach (var trendingResult in trendingResults)
                {
                    deserializedList.Add(unknownContentResponseDtoBuilder
                        .BeginBuilding(trendingResult.GeneralizedResult).AddViews(trendingResult).Build());
                }

                return deserializedList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }


        //! Users
        [HttpPost("saveUserView")]
        public async Task<IActionResult?> SaveUserView(SaveUserViewDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);

                if (requestingUser?.Username == request.Username)
                    return BadRequest("Error: You cannot save a view your own profile");

                var requestedUser = await _userService.GetUserByUsername(request.Username);
                var recommendation =
                    await _userRecommendationsService.GetUserRecommendationsDocumentById(requestedUser?.Id!);

                if (recommendation == null)
                {
                    await _userRecommendationsService.SaveUserView(requestedUser?.Id!);
                }
                else
                {
                    await _userRecommendationsService.AddViewToUser(requestedUser?.Id!,
                        recommendation.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingUsers")]
        public async Task<List<UserProfileDto>> GetTrendingUsers(GetTrendingUsersDto request)
        {
            try
            {
                var userProfileDtoBuilder = new UserProfileDtoBuilder();

                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);

                _userRecommendationsService.UpdateRecommendationsWeeklyViews();

                var trendingUsers =
                    await _userRecommendationsService.GetTrendingUsers(request.Username, request.Limit);
                if (trendingUsers == null) return new List<UserProfileDto>();

                var deserializedTrendingUsersInformation = new List<UserProfileDto>();
                foreach (var deserializedTrendingUser in trendingUsers)
                {
                    if (requestingUser?.Username != deserializedTrendingUser.User.Username)
                    {
                        var deserializedTrendingUserIsBeingFollowedAlready =
                            await _communityService.UserAlreadyBeingFollowed(deserializedTrendingUser.User.Id,
                                requestingUserId);

                        deserializedTrendingUsersInformation.Add(userProfileDtoBuilder.BeginBuilding(
                                deserializedTrendingUser.User.Name, deserializedTrendingUser.User.Username,
                                deserializedTrendingUser.User.ProfilePhotoUrl, deserializedTrendingUser)
                            .AddFollowed(deserializedTrendingUserIsBeingFollowedAlready)
                            .Build());
                    }
                }

                // Need to find other users that are not in the trending list
                // There can be repeating users, so the query has to take the amount of trending users into account for the limit
                var allUsers = await _communityService.GetUsersByNameOrUsername(request.Username,
                    request.Limit + deserializedTrendingUsersInformation.Count);
                int amountOfUsersToFind;
                if (allUsers.Count > request.Limit)
                {
                    amountOfUsersToFind = request.Limit - deserializedTrendingUsersInformation.Count;
                }
                else
                {
                    amountOfUsersToFind = allUsers.Count;
                }

                for (var i = 0; i < amountOfUsersToFind; i++)
                {
                    var currentUserViews = await
                        _userRecommendationsService.GetUserRecommendationsDocumentById(allUsers.ElementAt(i).Id);

                    var currentUserIsBeingFollowed = await
                        _communityService.UserAlreadyBeingFollowed(allUsers.ElementAt(i).Id, requestingUserId);

                    var currentUser = userProfileDtoBuilder.BeginBuilding(allUsers.ElementAt(i).Name,
                            allUsers.ElementAt(i).Username, allUsers.ElementAt(i).ProfilePhotoUrl, currentUserViews)
                        .AddFollowed(currentUserIsBeingFollowed)
                        .Build();

                    if (requestingUser?.Username != currentUser.Username)
                    {
                        deserializedTrendingUsersInformation.Add(currentUser);
                    }
                }

                return deserializedTrendingUsersInformation.GroupBy(userProfile => userProfile.Username)
                    .Select(group => group.First()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new List<UserProfileDto>();
            }
        }


        //! Playlists
        [HttpPost("savePlaylistView")]
        public async Task<IActionResult> SavePlaylistView(SavePlaylistViewDto request)
        {
            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return BadRequest("Error: User not found");

                var playlistRecommendationDocument =
                    await _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(request.PlaylistId);


                foreach (var userPlaylistId in requestingUser.UserPlaylistIds)
                {
                    if (userPlaylistId == new ObjectId(request.PlaylistId))
                    {
                        return BadRequest("Error: You cannot save a view to a playlist you own");
                    }
                }

                if (playlistRecommendationDocument == null)
                {
                    await _playlistRecommendationsService.SaveNewPlaylistView(request);
                }
                else
                {
                    await _playlistRecommendationsService.AddViewToPlaylist(request.PlaylistId,
                        playlistRecommendationDocument.WeeklyViewDates.Count,
                        playlistRecommendationDocument.TotalViewsAmount);
                }

                return Ok("View saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return BadRequest("Error: View not saved");
            }
        }

        [HttpPost("getTrendingPlaylists")]
        public async Task<List<PlaylistInformationDto>?> GetTrendingPlaylists(GetTrendingPlaylistsDto request)
        {
            bool PlaylistBelongsToRequestingUser(PlaylistDocument playlist, UserDocument user)
            {
                foreach (var userPlaylistId in user.UserPlaylistIds)
                {
                    if (new ObjectId(playlist.Id) == userPlaylistId)
                    {
                        return true;
                    }
                }

                return false;
            }

            try
            {
                var requestingUserId = _sessionTokensService.GetUserIdFromToken(request.SessionToken);
                var requestingUser = await _userService.GetUserById(requestingUserId);
                if (requestingUser == null) return null;
                var playlistInformationDtoBuilder = new PlaylistInformationDtoBuilder();

                _playlistRecommendationsService.UpdateRecommendationsWeeklyViews();
                var trendingPlaylists =
                    await _playlistRecommendationsService.GetTrendingPlaylists(request.PlaylistName, request.Limit);
                if (trendingPlaylists == null) return null;

                var deserializedTrendingPlaylistsInformation = new List<PlaylistInformationDto>();
                foreach (var deserializedTrendingPlaylist in trendingPlaylists)
                {
                    if (!PlaylistBelongsToRequestingUser(deserializedTrendingPlaylist.Playlist, requestingUser) &&
                        deserializedTrendingPlaylist.Playlist.Visibility == "Public")

                        deserializedTrendingPlaylistsInformation.Add(playlistInformationDtoBuilder.BeginBuilding(
                                deserializedTrendingPlaylist.Playlist.Id,
                                deserializedTrendingPlaylist.Playlist.Title,
                                deserializedTrendingPlaylist.Playlist.Description,
                                deserializedTrendingPlaylist.Playlist.ThumbnailUrl,
                                deserializedTrendingPlaylist.Playlist.ResultIds.Count)
                            .AddViews(deserializedTrendingPlaylist)
                            .Build());
                }

                // Need to find other playlists that are not in the trending list
                // There can be repeating playlists, so the query has to take the amount of trending playlists into account for the limit
                var allPlaylists = await _communityService.GetPlaylistsByTitle(request.PlaylistName,
                    request.Limit + deserializedTrendingPlaylistsInformation.Count);
                int amountOfPlaylistsToFind;
                if (allPlaylists.Count > request.Limit)
                {
                    amountOfPlaylistsToFind = request.Limit - deserializedTrendingPlaylistsInformation.Count;
                }
                else
                {
                    amountOfPlaylistsToFind = allPlaylists.Count;
                }

                for (var i = 0; i < amountOfPlaylistsToFind; i++)
                {
                    var currentPlaylistViews = await
                        _playlistRecommendationsService.GetPlaylistRecommendationsDocumentById(allPlaylists.ElementAt(i)
                            .Id);

                    var currentPlaylistInformation = playlistInformationDtoBuilder.BeginBuilding(
                            allPlaylists.ElementAt(i).Id,
                            allPlaylists.ElementAt(i).Title,
                            allPlaylists.ElementAt(i).Description,
                            allPlaylists.ElementAt(i).ThumbnailUrl, allPlaylists.ElementAt(i).ResultIds.Count)
                        .AddViews(currentPlaylistViews!).Build();

                    if (!PlaylistBelongsToRequestingUser(allPlaylists.ElementAt(i), requestingUser) &&
                        allPlaylists.ElementAt(i).Visibility == "Public")
                    {
                        deserializedTrendingPlaylistsInformation.Add(currentPlaylistInformation);
                    }
                }

                return deserializedTrendingPlaylistsInformation.GroupBy(playlist => playlist.Id)
                    .Select(group => group.First()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}