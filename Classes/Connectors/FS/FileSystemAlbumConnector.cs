using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemAlbumConnector : IMediaServerAlbumConnector
{
    private SQLiteAsyncConnection _database = null;
    
    public FileSystemAlbumConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public Task<Album[]> GetAllAlbumsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Album> GetAlbumAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Album[]> GetSimilarAlbumsAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalAlbumCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}