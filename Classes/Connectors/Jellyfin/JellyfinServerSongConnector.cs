using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin
{
    public class JellyfinServerSongConnector(JellyfinApiClient api, JellyfinSdkSettings clientSettings, UserDto user)
        : IMediaDataConnector
    {
        public SyncStatusInfo SyncStatusInfo { get; set; } = new();

        public void SetSyncStatusInfo(TaskStatus status, int percentage)
        {
            SyncStatusInfo.TaskStatus = status;
            SyncStatusInfo.StatusPercentage = percentage;
        }

        public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
            Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            BaseItemDtoQueryResult serverResults = await api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.IsFavorite = getFavourite;
                c.QueryParameters.SortBy = [setSortTypes];
                c.QueryParameters.SortOrder = [setSortOrder];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.PlayAccess, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks, ItemFields.DateCreated];
                c.QueryParameters.Limit = limit;
                c.QueryParameters.StartIndex = startIndex;
                c.QueryParameters.Recursive = true;
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            }, cancellationToken).ConfigureAwait(false);

            if (serverResults?.Items == null) return Array.Empty<Song>();

            return serverResults.Items.Select(dto => new Song(SongData.Builder(dto, clientSettings.ServerUrl))).ToArray();
        }

        public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            var songQueryResult = await api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.Ids = [id];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                c.QueryParameters.Recursive = true;
                c.QueryParameters.EnableImages = true;
            }, cancellationToken).ConfigureAwait(false);

            if (songQueryResult?.Items == null || !songQueryResult.Items.Any()) return new Song();

            return new Song(SongData.Builder(songQueryResult.Items.First(), clientSettings.ServerUrl));
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            // BaseItemDtoQueryResult result = await api.Songs[id].Similar.GetAsync(c =>
            // {
            //     c.QueryParameters.UserId = user.Id;
            //     c.QueryParameters.Limit = 10; // Example limit, adjust as necessary
            // }, cancellationToken).ConfigureAwait(false);
            //
            // if (result?.Items == null) return Array.Empty<Song>();
            //
            // return result.Items.Select(dto => Song.Builder(dto, clientSettings.ServerUrl)).ToArray();
            return [];
        }

        public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            BaseItemDtoQueryResult serverResults = await api.Items.GetAsync(c =>
            {
                c.QueryParameters.UserId = user.Id;
                c.QueryParameters.IsFavorite = getFavourite;
                c.QueryParameters.SortBy = [ItemSortBy.Name];
                c.QueryParameters.SortOrder = [SortOrder.Descending];
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
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

        public Task<bool> DeleteAsync(Guid[] id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddRange(BaseMusicItem[] musicItems, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
