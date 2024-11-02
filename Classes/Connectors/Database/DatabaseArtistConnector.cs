using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;


namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabaseArtistConnector : IMediaServerArtistConnector
{
    private SQLiteAsyncConnection _database = null;

    public DatabaseArtistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
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
