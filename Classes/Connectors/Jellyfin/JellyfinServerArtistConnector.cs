using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin
{
    public class JellyfinServerArtistConnector(JellyfinApiClient api, JellyfinSdkSettings clientSettings, UserDto user)
        : IMediaDataConnector
    {
        public SyncStatusInfo SyncStatusInfo { get; set; }

        public void SetSyncStatusInfo(TaskStatus status, int percentage)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
            Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            BaseItemDtoQueryResult artistResult = await api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.IsFavorite = getFavourite;
                c.QueryParameters.SortBy = [ItemSortBy.Name];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                c.QueryParameters.Limit = limit;
                c.QueryParameters.StartIndex = startIndex;
                c.QueryParameters.Recursive = true;
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            }, cancellationToken);
            if (artistResult?.Items == null) return [];
            var artists = artistResult.Items.Select(a =>
                new Artist(ArtistData.Builder(a, clientSettings.ServerUrl))).ToArray();
            return artists;
        }

        public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var runArtistInfo = api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.Ids = [id];
                c.QueryParameters.SortBy = [ItemSortBy.Name];
                c.QueryParameters.SortOrder = [SortOrder.Ascending];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                c.QueryParameters.Recursive = true;
                c.QueryParameters.Fields = [ItemFields.Overview];
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            }, cancellationToken);
            var runAlbumInfo = api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.ArtistIds = [id];
                c.QueryParameters.SortBy = [ItemSortBy.Name];
                c.QueryParameters.SortOrder = [SortOrder.Ascending];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                c.QueryParameters.Recursive = true;
                c.QueryParameters.Fields = [ItemFields.Overview];
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            }, cancellationToken);
            await Task.WhenAll(runArtistInfo, runAlbumInfo);
            var artistInfo = runArtistInfo.Result;
            var albumResult = runAlbumInfo.Result;
            if (artistInfo?.Items == null || albumResult?.Items == null || cancellationToken.IsCancellationRequested)
                return Artist.Empty;
            if (artistInfo.Items.Count <= 0 || cancellationToken.IsCancellationRequested) return Artist.Empty;

            foreach (BaseItemDto item in artistInfo.Items)
            {
                ArtistData artistData = ArtistData.Builder(item, clientSettings.ServerUrl);
                AlbumData[] albumData = albumResult.Items
                    .Select(album => AlbumData.Builder(album, clientSettings.ServerUrl)).ToArray();

                return new Artist(artistData, albumData);
            }

            return Artist.Empty;
        }

        public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            BaseItemDtoQueryResult result = await api.Artists[id.ToString()].Similar.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.Limit = setLimit;
            }, cancellationToken);
            if (result?.Items == null) return [];
            return [];
        }

        public async Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            BaseItemDtoQueryResult serverResults = await api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.IsFavorite = getFavourite;
                c.QueryParameters.SortBy = [ItemSortBy.Name];
                c.QueryParameters.SortOrder = [SortOrder.Descending];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                c.QueryParameters.Limit = 1;
                c.QueryParameters.StartIndex = 0;
                c.QueryParameters.Recursive = true;
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            }, cancellationToken).ConfigureAwait(false);
            return serverResults?.TotalRecordCount ?? 0;
        }

        public Task<bool> DeleteAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}