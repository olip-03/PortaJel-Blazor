using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabasePlaylistConnector : IMediaServerPlaylistConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabasePlaylistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task<Playlist[]> GetAllPlaylistsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        limit ??= 50;
        List<PlaylistData> filteredCache = [];
        filteredCache.AddRange(await _database.Table<PlaylistData>()
            .OrderByDescending(playlist => playlist.Name)
            .Take(limit.Value).ToListAsync().ConfigureAwait(false));
        return filteredCache.Select(dbItem => new Playlist(dbItem)).ToArray();
    }

    public async Task<Playlist> GetPlaylistAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        PlaylistData playlistDbItem =
            await _database.Table<PlaylistData>().Where(p => p.Id == id).FirstOrDefaultAsync();
        var songData = await _database.Table<SongData>().Where(song => playlistDbItem.GetSongIds().Contains(song.Id)).ToArrayAsync();
        return new Playlist(playlistDbItem, songData);
    }

    public async Task<int> GetTotalPlaylistCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        return await _database.Table<PlaylistData>().CountAsync();
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
    
    public async Task<bool> AddRange(Playlist[] playlists, CancellationToken cancellationToken = default)
    {
        foreach (var p in playlists)
        {
            await _database.InsertOrReplaceAsync(p.GetBase, playlists.First().GetBase.GetType());
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }
        return true;
    }
}