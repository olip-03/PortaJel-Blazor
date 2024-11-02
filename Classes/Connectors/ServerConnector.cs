using System.Diagnostics;
using PortaJel_Blazor.Classes.Connectors.Spotify;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

//  https://media.olisshittyserver.xyz/api-docs/swagger/index.html
public class ServerConnector : IMediaServerConnector
{
    private readonly List<IMediaServerConnector> _servers = [];
    public IMediaServerAlbumConnector Album { get; set; } 
    public IMediaServerArtistConnector Artist { get; set; }
    public IMediaServerSongConnector Song { get; set; }
    public IMediaServerPlaylistConnector Playlist { get; set; }
    public IMediaServerGenreConnector Genre { get; set; }
    public Dictionary<ConnectorDtoTypes, bool> SupportedReturnTypes { get; set; } =
        new()
        {
            { ConnectorDtoTypes.Album, true },
            { ConnectorDtoTypes.Artist, true },
            { ConnectorDtoTypes.Song, true },
            { ConnectorDtoTypes.Playlist, true },
            { ConnectorDtoTypes.Genre, false },
        };

    public Dictionary<string, ConnectorProperty> Properties { get; set; }
    public TaskStatus SyncStatus { get; set; } = TaskStatus.WaitingToRun;
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
        
        return new AuthenticationResponse();
    }

    public async Task<bool> IsUpToDateAsync(CancellationToken cancellationToken = default)
    {
        SyncStatus = TaskStatus.Running;
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
        
        SyncStatus = t.Status;
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
        SyncStatus = TaskStatus.Running;
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
        
        SyncStatus = t.Status;
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

    public string GetUsername()
    {
        throw new NotImplementedException("Unable to get username from main server connector. Please call this method specifying the server!");
    }
    
    public string GetUsername(string server)
    {
        return _servers.FirstOrDefault(s => s.GetAddress() == server)?.GetUsername();
    }

    public string GetPassword()
    {
        throw new NotImplementedException("Unable to get password from main server connector. Please call this method specifying the server!");
    }
    
    public string GetPassword(string server)
    {
        return _servers.FirstOrDefault(s => s.GetAddress() == server)?.GetPassword();
    }

    public string GetAddress()
    {
        throw new NotImplementedException("Unable to get address from main server connector. Please call this method specifying the server!");
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
}