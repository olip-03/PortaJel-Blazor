using Portajel.Connections.Data;
using Jellyfin.Sdk.Generated.Models;

namespace Portajel.Connections.Interfaces
{
    public interface IDbItemConnector
    {
        Task<BaseMusicItem[]> GetAllAsync(
            int? limit = null,
            int startIndex = 0,
            bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album,
            SortOrder setSortOrder = SortOrder.Ascending,
            Guid?[]? includeIds = null,
            Guid?[]? excludeIds = null,
            CancellationToken cancellationToken = default);
        Task<BaseMusicItem> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(
            bool? getFavourite = null,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteRangeAsync(
            Guid[] ids,
            CancellationToken cancellationToken = default);
        Task<bool> InsertAsync(
            BaseMusicItem musicItem,
            CancellationToken cancellationToken = default);
        Task<bool> InsertRangeAsync(
            BaseMusicItem[] musicItems,
            CancellationToken cancellationToken = default);
    }
}
