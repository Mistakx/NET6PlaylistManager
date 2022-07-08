using System.Text.Json.Nodes;
using SkyPlaylistManager.Models.DTOs.ContentResponses;

namespace SkyPlaylistManager.Models;

public class UserConnection
{
    public string UserId { get; set; }
    public List<string> ConnectionIds { get; set; } = new();
    public JsonObject? CurrentlyPlaying { get; set; }

    public static bool ContainsUserId(List<UserConnection> userConnections, string userId)
    {
        return userConnections.Any(x => x.UserId == userId);
    }
    
    public static bool ContainsConnectionId(List<UserConnection> userConnections, string connectionId)
    {
        return userConnections.Any(x => x.ConnectionIds.Contains(connectionId));
    }
    
    public static UserConnection GetUserConnectionByUserId(List<UserConnection> userConnections, string userId)
    {
        return userConnections.First(x => x.UserId == userId);
    }
    
    public static UserConnection GetUserConnectionByConnectionId(List<UserConnection> userConnections, string connectionId)
    {
        return userConnections.First(x => x.ConnectionIds.Contains(connectionId));
    }
    
    public static void RemoveUserId(List<UserConnection> userConnections, string userId)
    {
        userConnections.RemoveAll(x => x.UserId == userId);
    }
    
    public static void RemoveConnectionId(List<UserConnection> userConnections, string connectionId)
    {
        
        // Find the user connection that contains the connection id
        var userConnection = userConnections.First(x => x.ConnectionIds.Contains(connectionId));
        
        // Remove the connection id from the user connection
        userConnection.ConnectionIds.Remove(connectionId);
    }
    
    public static List<string> GetUserIds(List<UserConnection> userConnections)
    {
        return userConnections.Select(x => x.UserId).ToList();
    }

}