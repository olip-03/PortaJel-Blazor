using Portajel.Connections.Interfaces;
using Portajel.Connections.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;

namespace Portajel.Connections.Services.Spotify
{
    public class SpotifyServerPlaylistConnector : IMediaDataConnector
    {
        private IMediaDataConnector _mediaDataConnectorImplementation;

        public SyncStatusInfo SyncStatusInfo { get; set; }
        public MediaTypes MediaType { get; set; } = MediaTypes.Playlist;

        public void SetSyncStatusInfo(TaskStatus status, int percentage)
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
            Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
