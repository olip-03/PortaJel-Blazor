using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors.FS
{
    public class FileSystemArtistConnector : IMediaServerArtistConnector
    {
        public Task<Album[]> GetAllArtistAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Artist[]> GetAllArtistAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Artist, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Artist> GetArtistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Artist[]> GetSimilarArtistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalArtistCount(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
