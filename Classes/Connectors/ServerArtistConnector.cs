using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerArtistConnector : IMediaServerArtistConnector
{
    public ServerArtistConnector(List<IMediaServerConnector> servers)
    {
        
    }

    public Task<Album[]> GetAllArtistAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Album> GetArtistAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Album[]> GetSimilarArtistAsync()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalArtistCount()
    {
        throw new NotImplementedException();
    }
}