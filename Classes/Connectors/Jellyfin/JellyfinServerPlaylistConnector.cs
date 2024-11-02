using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin;

public class JellyfinServerPlaylistConnector : IMediaServerPlaylistConnector
{
    public async Task<Playlist[]> GetAllPlaylistsAsync()
    {
        // Implementation to fetch all playlists
        return await Task.FromResult(new Playlist[0]);
    }
    
    public async Task<Playlist> GetPlaylistAsync(int playlistId)
    {
        // Implementation to fetch a specific playlist by its ID
        return await Task.FromResult(new Playlist());
    }
    
    public async Task<int> GetTotalPlaylistCountAsync()
    {
        // Implementation to get the total count of playlists
        return await Task.FromResult(0);
    }
}