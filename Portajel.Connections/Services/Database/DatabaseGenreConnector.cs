using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Database;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using SQLite;

namespace Portajel.Connections.Services.Database;

public class DatabaseGenreConnector : IDbItemConnector
{
    private readonly SQLiteAsyncConnection _database = null;
    public MediaTypes MediaType { get; set; } = MediaTypes.Genre;
    public DatabaseGenreConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public Task<BaseMusicItem[]> GetAllAsync(
            int? limit = null,
            int startIndex = 0,
            bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album,
            SortOrder setSortOrder = SortOrder.Ascending,
            Guid?[]? includeIds = null,
            Guid?[]? excludeIds = null,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem[]>([]);
    }

    public Task<BaseMusicItem> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BaseMusicItem>(new Genre());
    }

    public Task<int> GetTotalCountAsync(
            bool? getFavourite = null,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteRangeAsync(
            Guid[] ids,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> InsertAsync(
            BaseMusicItem musicItem,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> InsertRangeAsync(
            BaseMusicItem[] musicItems,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}