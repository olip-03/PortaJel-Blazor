using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerSongConnector
    {
        Task<Song[]> GetAllSongsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            string serverUrl = "", CancellationToken cancellationToken = default);

        Task<Song> GetSongAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);

        Task<Song[]> GetSimilarSongsAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default);

        Task<int> GetTotalSongCountAsync(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default);
    }
}