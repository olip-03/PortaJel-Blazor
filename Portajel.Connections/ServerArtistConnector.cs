using System.Collections.Concurrent;
using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;

namespace Portajel.Connections;

public class ServerArtistConnector(List<IMediaServerConnector> servers) : IMediaDataConnector
{
    public MediaTypes MediaType { get; set; } = MediaTypes.Artist;
    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool? getFavourite = null,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending, Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var artists = new ConcurrentBag<Artist>();
        int failed = 0;
        
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Artist"].GetAllAsync(limit, startIndex, getFavourite, setSortTypes, setSortOrder, serverUrl: serverUrl, cancellationToken: cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var artist in toAdd.Result)
                    {
                        if (artist is Artist a)
                        {
                            artists.Add(a);
                        }
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
                Trace.WriteLine($"All artist request attempts succeeded: {artists.Count} artists retrieved.");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} artist request attempts failed!");
                break;
        }

        return artists.ToArray();
    }
    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        IMediaServerConnector server = connectors.FirstOrDefault(c => c.Key == serverUrl).Value;
        var artist = new ConcurrentBag<Artist>();
        int failed = 0;

        if (server == null)
        {
            // Create list of tasks from added connections
            var tasks = connectors.Select(connector => Task.Run(() =>
                {
                    try
                    {
                        // Get album data 
                        var t = connector.Value.GetDataConnectors()["Artist"].GetAsync(id, cancellationToken: cancellationToken);
                        t.Wait(cancellationToken);
                        if (t.Result != null)
                        {
                            if (t.Result is Artist a)
                            {
                                artist.Add(a);
                            }
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
            // Await functions
            Task t = Task.WhenAll(tasks);
            try
            {
                await t;
            }
            catch 
            { 
                // ignored
            }
            // Check response
            switch (t.Status)
            {
                case TaskStatus.Faulted:
                    Trace.WriteLine($"{failed} album request attempts failed!");
                    break;
            }
        }
        else
        {
            var result = await server.GetDataConnectors()["Artist"].GetAsync(id, serverUrl, cancellationToken);
            if (result is Artist a)
            {
                artist.Add(a);
            }
        }
        
        Trace.WriteLine($"Album request attempts succeeded: {artist.Count} albums retrieved.");
        return artist.First();
    }
    public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var artists = new ConcurrentBag<Artist>();
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Artist"].GetSimilarAsync(id, setLimit, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var artist in toAdd.Result)
                    {
                        if (artist is Artist a)
                        {
                            artists.Add(a);
                        }
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
                Trace.WriteLine($"Similar album request attempts succeeded: {artists.Count} albums retrieved.");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} similar album request attempts failed!");
                break;
        }

        return artists.ToArray();
    }

    public async Task<int> GetTotalCountAsync(bool? getFavourite = null, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        int totalCount = 0;
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Artist"].GetTotalCountAsync(getFavourite, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to total
                    Interlocked.Add(ref totalCount, toAdd.Result);
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
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} Total Album count request attempts failed!");
                break;
        }
        
        Trace.WriteLine($"Total Album count request succeeded: Value is {totalCount}.");
        return totalCount;
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