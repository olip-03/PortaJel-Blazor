using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemPlaylistConnector : IMediaServerPlaylistConnector
{
    public Task<Playlist[]> GetAllPlaylistsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Playlist> GetPlaylistAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalPlaylistCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}