using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerPlaylistConnector
    {
        Task<Playlist[]> GetAllPlaylistsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            string serverUrl = "", CancellationToken cancellationToken = default);
        Task<Playlist> GetPlaylistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);
        Task<int> GetTotalPlaylistCountAsync(bool getFavourite = false, string serverUrl = "", CancellationToken cancellationToken = default);
    }
}
