using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerPlaylistConnector : IMediaServerPlaylistConnector
{
    public ServerPlaylistConnector(List<IMediaServerConnector> servers)
    {
    
    } 

    public Task<Playlist[]> GetAllPlaylistsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Playlist> GetPlaylistAsync(int playlistId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalPlaylistCountAsync()
    {
        throw new NotImplementedException();
    }
}