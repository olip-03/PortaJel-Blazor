using System.Collections.Concurrent;
using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Connectors;

/// <summary>
/// Provides functionality to connect and interact with media server albums.
/// </summary>
public class ServerAlbumConnector(List<IMediaServerConnector> servers) : IMediaServerAlbumConnector
{
    /// <summary>
    /// Asynchronously retrieves all albums from the media server with the option to specify filters and sorting.
    /// </summary>
    /// <param name="limit">The maximum number of albums to retrieve. Defaults to 50.</param>
    /// <param name="startIndex">The starting index for album retrieval. Defaults to 0.</param>
    /// <param name="getFavourite">Indicates whether to retrieve only favourite albums. Defaults to false.</param>
    /// <param name="setSortTypes">Specifies the sorting criteria for the albums. Defaults to sorting by album.</param>
    /// <param name="setSortOrder">Specifies the order in which to sort the albums. Defaults to ascending order.</param>
    /// <param name="serverUrl">The URL of the server to retrieve albums from. Defaults to an empty string, which uses the default server.</param>
    /// <param name="cancellationToken">A token to cancel the operation. Defaults to None.</param>
    /// <returns>An array of <see cref="Album"/> retrieved from the server.</returns>
    public async Task<Album[]> GetAllAlbumsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var albums = new ConcurrentBag<Album>();
        int failed = 0;
        
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.Album.GetAllAlbumsAsync(limit, startIndex, getFavourite, setSortTypes, setSortOrder, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var album in toAdd.Result)
                    {
                        albums.Add(album);
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

    public async Task<Album> GetAlbumAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        IMediaServerConnector server = connectors.FirstOrDefault(c => c.Key == serverUrl).Value;
        var albums = new ConcurrentBag<Album>();
        int failed = 0;

        if (server == null)
        {
            // Create list of tasks from added connections
            var tasks = connectors.Select(connector => Task.Run(() =>
                {
                    try
                    {
                        // Get album data 
                        var t = connector.Value.Album.GetAlbumAsync(id, cancellationToken: cancellationToken);
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
            Album result = await server.Album.GetAlbumAsync(id, serverUrl, cancellationToken);
            albums.Add(result);
        }
        
        Trace.WriteLine($"Album request attempts succeeded: {albums.Count} albums retrieved.");
        return albums.First();
    }

    public async Task<Album[]> GetSimilarAlbumsAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        var albums = new ConcurrentBag<Album>();
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.Album.GetSimilarAlbumsAsync(id, serverUrl, cancellationToken);
                    toAdd.Wait(cancellationToken);
                    // Add to list 
                    foreach (var album in toAdd.Result)
                    {
                        albums.Add(album);
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

    public async Task<int> GetTotalAlbumCountAsync(bool getFavourite = false, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        int totalCount = 0;
        int failed = 0;
        var connectors = servers.ToDictionary(newServer => newServer.GetAddress(), newServer => (IMediaServerConnector)newServer);
        var tasks = connectors.Select(server => Task.Run(() =>
            {
                try
                {
                    // Get album data 
                    var toAdd = server.Value.Album.GetTotalAlbumCountAsync(getFavourite, serverUrl, cancellationToken);
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