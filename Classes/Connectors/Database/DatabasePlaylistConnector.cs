using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabasePlaylistConnector : IMediaServerPlaylistConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabasePlaylistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public async Task<Playlist[]> GetAllPlaylistsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        List<PlaylistData> filteredCache = [];
        filteredCache.AddRange(await _database.Table<PlaylistData>()
            .OrderByDescending(playlist => playlist.Name)
            .Take(limit).ToListAsync().ConfigureAwait(false));
        return filteredCache.Select(dbItem => new Playlist(dbItem)).ToArray();
    }

    public Task<Playlist> GetPlaylistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalPlaylistCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}