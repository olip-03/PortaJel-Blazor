using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerPlaylistConnector(List<IMediaServerConnector> servers) : IMediaServerPlaylistConnector
{
    public Task<Playlist[]> GetAllPlaylistsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    public Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}