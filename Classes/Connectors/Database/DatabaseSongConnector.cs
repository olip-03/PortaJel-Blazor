using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;
using System.Linq;

namespace PortaJel_Blazor.Classes.Connectors.Database
{
    public class DatabaseSongConnector : IMediaDataConnector
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly DatabaseConnector _databaseConnector;

        public DatabaseSongConnector(SQLiteAsyncConnection database, DatabaseConnector connector)
        {
            _database = database;
            _databaseConnector = connector;
        }

        public SyncStatusInfo SyncStatusInfo { get; set; }

        public void SetSyncStatusInfo(TaskStatus status, int percentage)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
            ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
            Guid?[] includeIds = null,
            Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();

            // Pre-optimize filters into hash sets if they exist
            HashSet<Guid?> includeHash = includeIds != null ? new HashSet<Guid?>(includeIds) : null;
            HashSet<Guid?> excludeHash = excludeIds != null ? new HashSet<Guid?>(excludeIds) : null;

            // Apply includeIds filter
            if (includeHash != null && includeHash.Any())
            {
                query = query.Where(song => includeHash.Contains(song.LocalId));
            }
            else if (includeHash != null)
            {
                return Array.Empty<BaseMusicItem>();
            }

            // Apply excludeIds filter
            if (excludeHash != null && excludeHash.Any())
            {
                query = query.Where(song => !excludeHash.Contains(song.LocalId));
            }
            else if (excludeHash != null)
            {
                return Array.Empty<BaseMusicItem>();
            }

            // Apply getFavourite filter
            if (getFavourite.HasValue)
            {
                bool fav = getFavourite.Value;
                query = query.Where(song => song.IsFavourite == fav);
            }

            // Apply sorting
            switch (setSortTypes)
            {
                case ItemSortBy.DateCreated:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.DateAdded)
                        : query.OrderByDescending(song => song.DateAdded);
                    break;
                case ItemSortBy.DatePlayed:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.DatePlayed)
                        : query.OrderByDescending(song => song.DatePlayed);
                    break;
                case ItemSortBy.Name:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.Name)
                        : query.OrderByDescending(song => song.Name);
                    break;
                case ItemSortBy.PlayCount:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.PlayCount)
                        : query.OrderByDescending(song => song.PlayCount);
                    break;
                case ItemSortBy.Random:
                    var allSongs = await query.ToListAsync().ConfigureAwait(false);
                    allSongs = allSongs.OrderBy(song => Guid.NewGuid()).ToList();
                    if (limit.HasValue)
                    {
                        allSongs = allSongs.Take(limit.Value).ToList();
                    }

                    // Bulk-load related data here after filtering
                    var albumIdsForRandom = allSongs.Select(s => (Guid?)s.LocalAlbumId).Distinct().ToArray();
                    var albumsForRandom = await _databaseConnector.Album.GetAllAsync(includeIds: albumIdsForRandom,
                        cancellationToken: cancellationToken);
                    var albumDictForRandom =
                        albumsForRandom.ToDictionary(a => a.LocalId, a => a.ToAlbum().GetBase);

                    var allArtistIdsForRandom = allSongs.SelectMany(s => s.GetArtistIds()).Distinct()
                        .Select(id => (Guid?)id).ToArray();
                    var artistsForRandom =
                        await _databaseConnector.Artist.GetAllAsync(includeIds: allArtistIdsForRandom,
                            cancellationToken: cancellationToken);
                    var artistDictForRandom =
                        artistsForRandom.ToDictionary(a => a.LocalId, a => a.ToArtist().GetBase);

                    List<BaseMusicItem> randomResults = new List<BaseMusicItem>(allSongs.Count);
                    foreach (var song in allSongs)
                    {
                        var albumBase = albumDictForRandom.TryGetValue(song.LocalAlbumId, out var alb) ? alb : null;
                        var artistBaseData = song.GetArtistIds()
                            .Select(id => artistDictForRandom.TryGetValue(id, out var artBase) ? artBase : null)
                            .Where(a => a != null)
                            .ToArray();

                        randomResults.Add(new Song(song, albumBase, artistBaseData));
                    }

                    return randomResults.ToArray();
                default:
                    query = setSortOrder == SortOrder.Ascending
                        ? query.OrderBy(song => song.Name)
                        : query.OrderByDescending(song => song.Name);
                    break;
            }

            // Apply pagination
            if (startIndex > 0)
            {
                query = query.Skip(startIndex);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            // Execute the query
            var filteredCache = await query.ToListAsync().ConfigureAwait(false);

            // Bulk-load albums and artists for all retrieved songs
            var albumIds = filteredCache.Select(s => (Guid?)s.LocalAlbumId).Distinct().ToArray();
            var albums =
                await _databaseConnector.Album.GetAllAsync(includeIds: albumIds, cancellationToken: cancellationToken);
            var albumDict = albums.ToDictionary(a => a.LocalId, a => a.ToAlbum().GetBase);

            var allArtistIds = filteredCache.SelectMany(s => s.GetArtistIds()).Distinct().Select(id => (Guid?)id)
                .ToArray();
            var artists =
                await _databaseConnector.Artist.GetAllAsync(includeIds: allArtistIds,
                    cancellationToken: cancellationToken);
            var artistDict = artists.ToDictionary(a => a.LocalId, a => a.ToArtist().GetBase);

            List<BaseMusicItem> toReturn = new List<BaseMusicItem>(filteredCache.Count);
            foreach (var song in filteredCache)
            {
                var albumBase = albumDict.TryGetValue(song.LocalAlbumId, out var alb) ? alb : null;
                var artistBaseData = song.GetArtistIds()
                    .Select(id => artistDict.TryGetValue(id, out var artBase) ? artBase : null)
                    .Where(a => a != null)
                    .ToArray();

                toReturn.Add(new Song(song, albumBase, artistBaseData));
            }

            return toReturn.ToArray();
        }


        public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var songData = await _database.Table<SongData>().Where(song => song.Id == id).FirstOrDefaultAsync();
            if (songData == null) return Song.Empty;

            var albumData = await _database.Table<AlbumData>().Where(album => songData.AlbumId == album.Id)
                .FirstOrDefaultAsync();
            var artistData = await _database.Table<ArtistData>()
                .Where(artist => songData.GetArtistIds().First() == artist.Id).FirstOrDefaultAsync();
            return new Song(songData, albumData, [artistData]);
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);
        }

        public Task<BaseMusicItem[]> GetSimilarAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<BaseMusicItem[]>([]);
        }

        public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            var query = _database.Table<SongData>();
            if (getFavourite == true)
                query = query.Where(song => song.IsFavourite);

            int total = await query.CountAsync();
            return total;
        }

        public async Task<bool> DeleteAsync(Guid id, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Delete associated songs
                var songs = await _database.Table<SongData>().Where(s => s.LocalId == id).ToListAsync();
                foreach (var song in songs)
                {
                    await _database.DeleteAsync(song);
                    Trace.WriteLine($"Deleted song with ID {song.LocalId} associated with album ID {id}.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error deleting song with ID {id}: {ex.Message}");
                return false; // Deletion failed
            }
        }

        public async Task<bool> DeleteAsync(Guid[] ids, string serverUrl = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var id in ids)
                {
                    // Find the album
                    var song = await _database.Table<SongData>().FirstOrDefaultAsync(s => s.LocalId == id);
                    if (song == null)
                    {
                        Trace.WriteLine($"Song with ID {id} not found.");
                        return false; // Stop if any album is not found
                    }

                    await _database.DeleteAsync(song);
                    Trace.WriteLine($"Deleted Song with ID {id}.");
                }

                return true; // All deletions succeeded
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error deleting albums: {ex.Message}");
                return false; // Deletion failed for one or more
            }
        }

        public async Task<bool> AddRange(BaseMusicItem[] songs, CancellationToken cancellationToken = default)
        {
            foreach (var baseMusicItem in songs)
            {
                if (baseMusicItem is Song { GetBase: not null } s)
                {
                    SongData song = s.GetBase;
                    song.BlurhashBase64 = baseMusicItem.ImgBlurhashBase64;
                    await _database.InsertOrReplaceAsync(song, s.GetBase.GetType());
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            return true;
        }
    }
}