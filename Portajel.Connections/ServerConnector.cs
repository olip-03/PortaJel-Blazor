using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Services;

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

    public string Name { get; } = "Server Connector";
    public string Description { get; } = "Base Server Connector. Contains the rest of them.";
    public string Image { get; } = "hub.png";
    public Dictionary<string, ConnectorProperty> Properties { get; set; } = [];
    public SyncStatusInfo SyncStatus { get; set; } = new();

    public ServerConnector()
    {
        Album = new ServerAlbumConnector(_servers);
        Artist = new ServerArtistConnector(_servers);
        Song = new ServerSongConnector(_servers);
        Playlist = new ServerPlaylistConnector(_servers);
        Genre = new ServerGenreConnector(_servers);
    }

    public async Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
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
        
        return AuthResponse.Ok();
    }

    public Task<bool> UpdateDb(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Cant update the main server hub. Please call StartSyncAsync.");
    }

    public async Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
    {
        int failed = 0;
        List<Task> syncJobs = new();
        var tasks = _servers.Select(server => Task.Run(() =>
        {
            try
            {
                if (server.Properties.TryGetValue("LastSyncDate", out ConnectorProperty? value))
                {
                    DateTime lastSyncDate = DateTime.Parse((string)value.Value);
                    if ((DateTime.Now - lastSyncDate).TotalDays >= 90) // 3 months ~= 90 days
                    {
                        server.UpdateDb();
                        return;
                    }
                }
                syncJobs.Add(server.StartSyncAsync(cancellationToken));
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
        catch { }

        foreach (Task sj in syncJobs)
        {
            await sj;
        }

        return true;
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