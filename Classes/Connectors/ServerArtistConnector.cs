using System.Collections.Concurrent;
using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

public class ServerArtistConnector(List<IMediaServerConnector> servers) : IMediaServerArtistConnector
{
    public async Task<Artist[]> GetAllArtistAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Artist, SortOrder setSortOrder = SortOrder.Ascending, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        var artists = new ConcurrentBag<Artist>();
        int failed = 0;
        
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.Artist.GetAllArtistAsync(limit, startIndex, getFavourite, setSortTypes, setSortOrder, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var artist in toAdd.Result)
                    {
                        artists.Add(artist);
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

    public async Task<Artist> GetArtistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
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
                        var t = connector.Value.Artist.GetArtistAsync(id, cancellationToken: cancellationToken);
                        t.Wait(cancellationToken);
                        if (t.Result != null)
                        {
                            artist.Add(t.Result);
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
            Artist result = await server.Artist.GetArtistAsync(id, serverUrl, cancellationToken);
            artist.Add(result);
        }
        
        Trace.WriteLine($"Album request attempts succeeded: {artist.Count} albums retrieved.");
        return artist.First();
    }

    public async Task<Artist[]> GetSimilarArtistAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var artists = new ConcurrentBag<Artist>();
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.Artist.GetSimilarArtistAsync(id, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var artist in toAdd.Result)
                    {
                        artists.Add(artist);
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

    public async Task<int> GetTotalArtistCount(bool getFavourite = false, string serverUrl = "",
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
                    var toAdd = server.Value.Artist.GetTotalArtistCount(getFavourite, serverUrl, cancellationToken);
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
}