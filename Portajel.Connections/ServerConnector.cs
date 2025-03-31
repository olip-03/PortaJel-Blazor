using System.Diagnostics;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Services;

namespace Portajel.Connections;

//  https://media.olisshittyserver.xyz/api-docs/swagger/index.html
public class ServerConnector : IServerConnector
{
    public List<IMediaServerConnector> Servers { get; set; } = [];
    public Dictionary<string, ConnectorProperty> Properties { get; set; } = [];
    public ServerConnector()
    {
        
    }
    public async Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        int failed = 0;
        var tasks = Servers.Select(server => Task.Run(() =>
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
    public async Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
    {
        int failed = 0;
        List<Task> syncJobs = new();
        var tasks = Servers.Select(server => Task.Run(() =>
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
    public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0,
        ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<BaseMusicItem>());
    }

    public ServerConnectorSettings GetSettings()
    {
        throw new NotImplementedException();
    }
}