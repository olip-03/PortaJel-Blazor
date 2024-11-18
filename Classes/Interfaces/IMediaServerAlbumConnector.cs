using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerAlbumConnector
    {
        Task<Album[]> GetAllAlbumsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            string serverUrl = "", CancellationToken cancellationToken = default);
        Task<Album> GetAlbumAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);
        Task<Album[]> GetSimilarAlbumsAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default);
        Task<int> GetTotalAlbumCountAsync(bool getFavourite = false, string serverUrl = "", CancellationToken cancellationToken = default);
    }
}