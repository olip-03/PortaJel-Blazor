﻿using Jellyfin.Sdk;
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

            Album = new DatabaseAlbumConnector(_database);
            Artist = new DatabaseArtistConnector(_database);
            Song = new DatabaseSongConnector(_database);
            Playlist = new DatabasePlaylistConnector(_database);
            Genre = new DatabaseGenreConnector(_database);
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

        public Task<BaseMusicItem[]> SearchAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Array.Empty<BaseMusicItem>());
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
    }
}