using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin;

public class JellyfinServerPlaylistConnector(JellyfinApiClient api, JellyfinSdkSettings clientSettings, UserDto user)
    : IMediaServerPlaylistConnector
{
    public async Task<Playlist[]> GetAllPlaylistsAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var playlistResult = await api.Items.GetAsync(c =>
        {
            c.QueryParameters.UserId = user.Id;
            c.QueryParameters.SortBy = [setSortTypes];
            c.QueryParameters.SortOrder = [setSortOrder];
            c.QueryParameters.IsFavorite = getFavourite;
            c.QueryParameters.IncludeItemTypes = [BaseItemKind.Playlist];
            c.QueryParameters.Limit = limit;
            c.QueryParameters.StartIndex = startIndex;
            c.QueryParameters.Recursive = true;
            c.QueryParameters.EnableImages = true;
            c.QueryParameters.EnableTotalRecordCount = true;
            c.QueryParameters.Fields = [ItemFields.Path];
        }, cancellationToken).ConfigureAwait(false);
        if (playlistResult == null || cancellationToken.IsCancellationRequested) return [];
        if (playlistResult.Items == null || cancellationToken.IsCancellationRequested) return [];
        return playlistResult.Items.Select(p => new Playlist(PlaylistData.Builder(p, clientSettings.ServerUrl)))
            .ToArray();
    }

    public async Task<Playlist> GetPlaylistAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to fetch a specific playlist by its ID
        var playlistQueryResult = api.Items.GetAsync(c =>
        {
            c.QueryParameters.UserId = user.Id;
            c.QueryParameters.Ids = [id];
            c.QueryParameters.Recursive = true;
            c.QueryParameters.EnableImages = true;
            c.QueryParameters.Fields = [ItemFields.Path];
        }, cancellationToken);
        var playlistSongResult = api.Items.GetAsync(c =>
        {
            c.QueryParameters.UserId = user.Id;
            c.QueryParameters.ParentId = id;
            c.QueryParameters.Fields =
                [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
            c.QueryParameters.EnableImages = true;
        }, cancellationToken);
        await Task.WhenAll(playlistQueryResult, playlistSongResult);
        if (playlistQueryResult.Result?.Items == null) return Playlist.Empty;
        if (playlistSongResult.Result?.Items == null) return Playlist.Empty;
        var songData = playlistSongResult.Result?.Items.Select(song => SongData.Builder(song, clientSettings.ServerUrl))
            .ToArray();
        return await Task.FromResult(new Playlist());
    }

    public async Task<int> GetTotalPlaylistCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to get the total count of playlists
        return await Task.FromResult(0);
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