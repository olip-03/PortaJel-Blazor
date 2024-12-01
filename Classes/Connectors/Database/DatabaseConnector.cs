using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using PortaJel_Blazor.Classes.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using PortaJel_Blazor.Classes.Database;
using SQLite;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PortaJel_Blazor.Classes.Connectors.Database;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Database
{
    public class DatabaseConnector : IMediaServerConnector
    {
        private static readonly string MainDir = Path.Combine(FileSystem.Current.AppDataDirectory, "Database.sqlite");

        private const SQLiteOpenFlags DbFlags =
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        private readonly SQLiteAsyncConnection _database = new SQLiteAsyncConnection(MainDir, DbFlags);
        public IMediaDataConnector Album { get; set; }
        public IMediaDataConnector Artist { get; set; }
        public IMediaDataConnector Song { get; set; }
        public IMediaDataConnector Playlist { get; set; }
        public IMediaDataConnector Genre { get; set; }

        public Dictionary<string, IMediaDataConnector> GetDataConnectors() => new()
        {
            { "Album", Album },
            { "Artist", Artist },
            { "Song", Song },
            { "Playlist", Playlist },
            { "Genre", Genre }
        };

        public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; } =
            new()
            {
                { ConnectorDtoTypes.Album, true },
                { ConnectorDtoTypes.Artist, true },
                { ConnectorDtoTypes.Song, true },
                { ConnectorDtoTypes.Playlist, true },
                { ConnectorDtoTypes.Genre, false },
            };

        public Dictionary<string, ConnectorProperty> Properties { get; set; } = new();
        public SyncStatusInfo SyncStatus { get; set; } = new();

        public DatabaseConnector()
        {
            Trace.WriteLine($"Database created at {MainDir}");

            _database.CreateTableAsync<AlbumData>().Wait();
            _database.CreateTableAsync<SongData>().Wait();
            _database.CreateTableAsync<ArtistData>().Wait();
            _database.CreateTableAsync<PlaylistData>().Wait();

            Album = new DatabaseAlbumConnector(_database, this);
            Artist = new DatabaseArtistConnector(_database, this);
            Song = new DatabaseSongConnector(_database, this);
            Playlist = new DatabasePlaylistConnector(_database, this);
            Genre = new DatabaseGenreConnector(_database, this);
        }

        public Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuthenticationResponse.Unneccesary());
        }

        public Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // If no search term is provided, return an empty array.
                return Array.Empty<BaseMusicItem>();
            }

            searchTerm = FormatString(searchTerm);

            var resultList = new List<BaseMusicItem>();

            // Iterate over each data connector
            foreach (var connectorPair in GetDataConnectors())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var connectorName = connectorPair.Key;
                var dataConnector = connectorPair.Value;
                
                switch (connectorName)
                {
                    case "Album":
                        var matchingAlbums = (await _database.Table<AlbumData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingAlbums.Select(item => new Album(item)));
                        break;
                    case "Artist":
                        var matchingArtists = (await _database.Table<ArtistData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingArtists.Select(item => new Artist(item)));
                        break;
                    case "Song":
                        var matchingSongs = (await _database.Table<SongData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingSongs.Select(item => new Song(item)));
                        break;
                    case "Playlist":
                        var matchingPlaylists = (await _database.Table<PlaylistData>().ToListAsync().ConfigureAwait(false))
                            .Where(item => FormatString(item.Name).Contains(searchTerm))
                            .ToList();
                        resultList.AddRange(matchingPlaylists.Select(item => new Playlist(item)));
                        break;
                    case "Genre":
                        // var matchingItems = (await _database.Table<GenreData>().ToListAsync().ConfigureAwait(false))
                        //     .Where(item => FormatString(item.Name).Contains(searchTerm))
                        //     .ToList();
                        // resultList.AddRange(matchingItems.Select(item => new Genre(item)));
                        break;
                }
            }

            // Now, apply sorting on the combined results
            IEnumerable<BaseMusicItem> sortedResult;

            switch (setSortTypes)
            {
                case ItemSortBy.DateCreated:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.DateAdded)
                        : resultList.OrderByDescending(item => item.DateAdded);
                    break;
                case ItemSortBy.DatePlayed:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.DatePlayed)
                        : resultList.OrderByDescending(item => item.DatePlayed);
                    break;
                case ItemSortBy.Name:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.Name)
                        : resultList.OrderByDescending(item => item.Name);
                    break;
                case ItemSortBy.PlayCount:
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.PlayCount)
                        : resultList.OrderByDescending(item => item.PlayCount);
                    break;
                default:
                    // Default sorting by Name
                    sortedResult = setSortOrder == SortOrder.Ascending
                        ? resultList.OrderBy(item => item.Name)
                        : resultList.OrderByDescending(item => item.Name);
                    break;
            }

            // Apply limit and offset
            if (limit.HasValue)
            {
                sortedResult = sortedResult.Skip(startIndex).Take(limit.Value);
            }

            // Return the result as an array
            return sortedResult.ToArray();
        }


        public string GetUsername()
        {
            throw new NotImplementedException();
        }

        public string GetPassword()
        {
            throw new NotImplementedException();
        }

        public string GetAddress()
        {
            throw new NotImplementedException();
        }

        public string GetProfileImageUrl()
        {
            throw new NotImplementedException();
        }

        public UserCredentials GetUserCredentials()
        {
            return new UserCredentials(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty);
        }

        public MediaServerConnection GetConnectionType()
        {
            return MediaServerConnection.Database;
        }

        public SQLiteAsyncConnection GetDatabaseConnection()
        {
            return _database;
        }
        
        private string FormatString(string toFormat)
        {
            if (string.IsNullOrEmpty(toFormat))
                return string.Empty;
            string result = Regex.Replace(toFormat, @"[^a-zA-Z0-9]", "");
            result = result.ToLowerInvariant();
            return result;
        }
    }
}