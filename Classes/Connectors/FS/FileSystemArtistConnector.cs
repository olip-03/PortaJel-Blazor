using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemArtistConnector : IMediaServerArtistConnector
{
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