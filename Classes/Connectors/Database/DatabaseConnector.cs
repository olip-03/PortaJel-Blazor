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
using PortaJel_Blazor.Classes.Connectors.Database;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Database
{
    public class DatabaseConnector : IMediaServerConnector
    {
        private static readonly string MainDir = Path.Combine(FileSystem.Current.AppDataDirectory, "Database.bi");
        private const SQLiteOpenFlags DbFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        private static readonly SQLiteAsyncConnection Database = new SQLiteAsyncConnection(MainDir, DbFlags);
        
        public IMediaServerAlbumConnector Album { get; set; } = new DatabaseAlbumConnector(Database);
        public IMediaServerArtistConnector Artist { get; set; } = new DatabaseArtistConnector(Database);
        public IMediaServerSongConnector Song { get; set; } = new DatabaseSongConnector(Database);
        public IMediaServerPlaylistConnector Playlist { get; set; } = new DatabasePlaylistConnector(Database);
        public IMediaServerGenreConnector Genre { get; set; } = new DatabaseGenreConnector(Database);
        
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
        public TaskStatus SyncStatus { get; set; }  = TaskStatus.WaitingToRun;
        
        public async Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            await Database.CreateTableAsync<AlbumData>();
            await Database.CreateTableAsync<SongData>();
            await Database.CreateTableAsync<ArtistData>();
            await Database.CreateTableAsync<PlaylistData>();
            
            return AuthenticationResponse.Unneccesary();
        }
        
        public Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
        {
            SyncStatus = TaskStatus.Running;
            throw new NotImplementedException();
        }
        
        public Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default)
        {
            SyncStatus = TaskStatus.Running;
            throw new NotImplementedException();
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
    }
}
