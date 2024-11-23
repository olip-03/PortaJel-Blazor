using System.Collections.Concurrent;
using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

/// <summary>
/// Provides functionality to connect and interact with media server albums.
/// </summary>
public class ServerAlbumConnector(List<IMediaServerConnector> servers) : IMediaDataConnector
{
    public SyncStatusInfo SyncStatusInfo { get; set; }

    public void SetSyncStatusInfo(TaskStatus status, int percentage)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseMusicItem[]> GetAllAsync(int? limit = null, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        Guid?[] includeIds = null,
        Guid?[] excludeIds = null, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var albums = new ConcurrentBag<Album>();
        int failed = 0;

        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(),
            newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Album"].GetAllAsync(limit, startIndex, getFavourite, setSortTypes,
                        setSortOrder, serverUrl: serverUrl, cancellationToken: cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var album in toAdd.Result)
                    {
                        if (album is Album a)
                        {
                            albums.Add(a);
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
                Trace.WriteLine($"All album request attempts succeeded: {albums.Count} albums retrieved.");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} album request attempts failed!");
                break;
        }

        return albums.ToArray();
    }

    public async Task<BaseMusicItem> GetAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(),
            newServer => (IMediaServerConnector)newServer);
        IMediaServerConnector server = connectors.FirstOrDefault(c => c.Key == serverUrl).Value;
        var albums = new ConcurrentBag<BaseMusicItem>();
        int failed = 0;

        if (server == null)
        {
            // Create list of tasks from added connections
            var tasks = connectors.Select(connector => Task.Run(() =>
                {
                    try
                    {
                        // Get album data 
                        var t = connector.Value.GetDataConnectors()["Album"].GetAsync(id, cancellationToken: cancellationToken);
                        t.Wait(cancellationToken);
                        if (t.Result != null)
                        {
                            albums.Add(t.Result);
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
            var result = await server.GetDataConnectors()["Album"].GetAsync(id, serverUrl, cancellationToken);
            if (result is Album a)
            {
                albums.Add(a);
            }
        }

        Trace.WriteLine($"Album request attempts succeeded: {albums.Count} albums retrieved.");
        return albums.First();
    }

    public async Task<BaseMusicItem[]> GetSimilarAsync(Guid id, int setLimit, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var albums = new ConcurrentBag<Album>();
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(),
            newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Album"].GetSimilarAsync(id, 50, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var album in toAdd.Result)
                    {
                        if (album is Album a)
                        {
                            albums.Add(a);
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
                Trace.WriteLine($"Similar album request attempts succeeded: {albums.Count} albums retrieved.");
                break;
            case TaskStatus.Faulted:
                Trace.WriteLine($"{failed} similar album request attempts failed!");
                break;
        }

        return albums.ToArray();
    }

    public async Task<int> GetTotalCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        int totalCount = 0;
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(),
            newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.GetDataConnectors()["Album"].GetTotalCountAsync(getFavourite, serverUrl, cancellationToken);
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
}