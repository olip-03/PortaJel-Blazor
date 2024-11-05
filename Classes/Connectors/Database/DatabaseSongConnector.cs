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
    public Task<Song[]> GetAllSongsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task<Song> GetSongAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task<Song[]> GetSimilarSongsAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task<int> GetTotalSongCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}