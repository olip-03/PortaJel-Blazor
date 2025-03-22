using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Services.Database;
using Portajel.Connections.Services.FS;
using Portajel.Connections.Services.Jellyfin;
using Portajel.Connections.Services.Spotify;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;

namespace Portajel.Connections;

//  https://media.olisshittyserver.xyz/api-docs/swagger/index.html
public class ServerConnector : IMediaServerConnector
{
    private readonly List<IMediaServerConnector> _servers = [];
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

    public Dictionary<MediaTypes, bool> SupportedReturnTypes { get; set; } =
        new()
        {
            { MediaTypes.Album, true },
            { MediaTypes.Artist, true },
            { MediaTypes.Song, true },
            { MediaTypes.Playlist, true },
            { MediaTypes.Genre, false }
        };
    public Dictionary<string, ConnectorProperty> Properties { get; set; }
    public SyncStatusInfo SyncStatus { get; set; } = new();
    public ServerConnector()
    {
        Album = new ServerAlbumConnector(_servers);
        Artist = new ServerArtistConnector(_servers);
        Song = new ServerSongConnector(_servers);
        Playlist = new ServerPlaylistConnector(_servers);
        Genre = new ServerGenreConnector(_servers);
    }
    public async Task<AuthenticationResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        int failed = 0;
        var tasks = _servers.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.AuthenticateAsync(cancellationToken);
                    toAdd.Wait(cancellationToken);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Interlocked.Increment(ref failed);
                    throw;
                }
            }, cancellationToken))
            .ToList();
        Task t = Task.WhenAll(tasks);
        try
        {
            await t;
        }
        catch 
        { 
            // ignored
        }
        
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                Trace.WriteLine($"All connections successfully authenticated");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} album request attempts failed!");
                break;
        }
        
        return AuthenticationResponse.Ok();
    }
    public async Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
    {
        int isUpToDate = 0;
        int failed = 0;
        var tasks = _servers.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var status = server.IsUpToDateAsync(cancellationToken);
                    status.Wait(cancellationToken);
                    if (status.Result)
                    {
                        Interlocked.CompareExchange(ref isUpToDate, 1, 0);
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref isUpToDate, 0, 1);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Interlocked.Increment(ref failed);
                    throw;
                }
            }, cancellationToken))
            .ToList();
        
        Task t = Task.WhenAll(tasks);
        try
        {
            await t;
        }
        catch 
        { 
            // ignored
        }
        
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                Trace.WriteLine($"All connections successfully authenticated");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} album request attempts failed!");
                break;
        }
        
        return isUpToDate == 1 ? true : false;
    }
    public async Task<bool> BeginSyncAsync(CancellationToken cancellationToken = default)
    {
        int failed = 0;
        var tasks = _servers.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var status = server.BeginSyncAsync(cancellationToken);
                    status.Wait(cancellationToken);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    Interlocked.Increment(ref failed);
                    throw;
                }
            }, cancellationToken))
            .ToList();
        Task t = Task.WhenAll(tasks);
        try
        {
            await t;
        }
        catch 
        { 
            // ignored
        }
        
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                Trace.WriteLine($"All connections successfully synced");
                break;
            case TaskStatus.Faulted:
                char e = failed > 1 ? 's' : '\0';
                Trace.TraceError($"{failed} album sync{e} failed!");
                break;
        }
        
        return failed > 0;
    }
    
    public async Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
    {
        await Task.Delay(100);
        return true;
    }
    
    public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
        ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<BaseMusicItem>());
    }
    
    [Obsolete("This method will throw an Error! Please call GetUsername(string server)!")]
    public string GetUsername()
    {
        throw new NotImplementedException("Unable to get username from main server connector. Please call this method specifying the server!");
    }
    
    public string GetUsername(string server)
    {
        return _servers.FirstOrDefault(s => s.GetAddress() == server)?.GetUsername();
    }
    
    [Obsolete("This method will throw an Error! Please call GetUsername(string server)!")]
    public string GetPassword()
    {
        throw new NotImplementedException("Unable to get password from main server connector. Please call this method specifying the server!");
    }
    
    public string GetPassword(string server)
    {
        return _servers.FirstOrDefault(s => s.GetAddress() == server)?.GetPassword();
    }
    
    [Obsolete("This method will throw an Error! Please call GetUsername(string server)!")]
    public string GetAddress()
    {
        throw new NotImplementedException("Unable to get address from main server connector. Please call this method specifying the server!");
    }

    public string GetProfileImageUrl()
    {
        throw new NotImplementedException();
    }

    public UserCredentials GetUserCredentials()
    {
        return new UserCredentials("", "", "", "", "", "");
    }

    public MediaServerConnection GetConnectionType()
    {
        return MediaServerConnection.ServerConnector;
    }
    
    public string GetAddress(string server)
    {
        return _servers.FirstOrDefault(s => s.GetAddress() == server)?.GetAddress();
    }
    
    public void AddServer(IMediaServerConnector server)
    {
        _servers.Add(server);
    }
    
    public void RemoveServer(IMediaServerConnector server)
    {
        _servers.Remove(server);
    }

    public void RemoveServer(string address)
    {
        _servers.Remove(_servers.First(s => s.GetAddress() == address));
    }

    public IMediaServerConnector[] GetServers()
    {
        return _servers.ToArray();
    }
    public ServerConnectorSettings GetSettings()
    {
        return new ServerConnectorSettings(this, _servers.ToArray());
    }
    public UserCredentials[] GetAllUserCredentials()
    {
        return _servers.Select(s => s.GetUserCredentials()).ToArray();
    }
}