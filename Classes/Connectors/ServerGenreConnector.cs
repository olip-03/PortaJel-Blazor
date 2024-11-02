using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerGenreConnector : IMediaServerGenreConnector
{
    public ServerGenreConnector (List<IMediaServerConnector> servers)
    {
    
    }
    
    public Task<Genre[]> GetAllGenresAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Genre> GetGenreAsync(int genreId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalGenreCountAsync()
    {
        throw new NotImplementedException();
    }
}