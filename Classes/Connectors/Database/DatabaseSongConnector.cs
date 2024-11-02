using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabaseSongConnector : IMediaServerSongConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabaseSongConnector(SQLiteAsyncConnection database)
    {
        _database = database;
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