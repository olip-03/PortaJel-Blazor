using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerSongConnector : IMediaServerSongConnector
{
    public ServerSongConnector(List<IMediaServerConnector> servers)
    {
        
    }

    public Task<Song[]> GetAllSongsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Song> GetSongAsync(int songId)
    {
        throw new NotImplementedException();
    }

    public Task<Song[]> GetSimilarSongsAsync(int songId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalSongCountAsync()
    {
        throw new NotImplementedException();
    }
}