using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Java.IO;
using Java.Nio.Channels;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Extensions.DependencyInjection;
using Portajel.Connections;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Connections.Services.Database;
using Portajel.Droid.Services;
using Portajel.Structures.Functional;
using PortaJel.Droid.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Portajel.Services
{
    public class DroidServerConnector : IServerConnector
    {
        private ServiceCollection _serviceConnection = null!;
        public DroidServerConnector(DroidServiceController serverConnectior)
        {
            _serviceConnection = serverConnectior.AppServiceConnection;
        }
        public List<IMediaServerConnector> Servers { get => _serviceConnection.Binder.Server.Servers; }

        public Dictionary<string, ConnectorProperty> Properties => _serviceConnection.Binder.Server.Properties;

        public async Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            return await _serviceConnection.Binder.Server.AuthenticateAsync();
        }

        public ServerConnectorSettings GetSettings()
        {
            return _serviceConnection.Binder.Server.GetSettings();
        }

        public async Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0, ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending, CancellationToken cancellationToken = default)
        {
            return await _serviceConnection.Binder.Server.SearchAsync(searchTerm, limit, startIndex, setSortTypes, setSortOrder, cancellationToken);
        }

        public async Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
        {
            return await _serviceConnection.Binder.Server.StartSyncAsync(cancellationToken);
        }
        public void AddServer(IMediaServerConnector server)
        {
            _serviceConnection.Binder.Server.Servers.Add(server);
        }
        public void RemoveServer(IMediaServerConnector server)
        {
            Servers.Remove(server);
            _serviceConnection.Binder.Server.Servers.Remove(server);
        }
        public void RemoveServer(string address)
        {
            _serviceConnection.Binder.Server.Servers.Remove(Servers.First(s => s.GetAddress() == address));

        }
        public IMediaServerConnector[] GetServers()
        {
            return _serviceConnection.Binder.Server.Servers.ToArray();
        }
    }
}
