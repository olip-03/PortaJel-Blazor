using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerArtistConnector
    {
        Task<Artist[]> GetAllArtistAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Artist, SortOrder setSortOrder = SortOrder.Ascending,
            string serverUrl = "", CancellationToken cancellationToken = default);

        Task<Artist> GetArtistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default);

        Task<Artist[]> GetSimilarArtistAsync(Guid id, int setLimit, string serverUrl = "",
            CancellationToken cancellationToken = default);

        Task<int> GetTotalArtistCount(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default);
    }
}