using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Droid.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel.Droid.Services.MediaServer
{
    public class DroidServerConnector : IMediaServerConnector
    {
        private ServiceController controller;

        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public string Image => throw new NotImplementedException();

        public Dictionary<MediaTypes, bool> SupportedReturnTypes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, ConnectorProperty> Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SyncStatusInfo SyncStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DroidServerConnector(ServiceController _ctrl)
        {
            controller = _ctrl;
        }

        public Task<AuthResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public string GetAddress()
        {
            throw new NotImplementedException();
        }

        public MediaServerConnection GetConnectionType()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, IMediaDataConnector> GetDataConnectors()
        {
            throw new NotImplementedException();
        }

        public string GetPassword()
        {
            throw new NotImplementedException();
        }

        public string GetProfileImageUrl()
        {
            throw new NotImplementedException();
        }

        public UserCredentials GetUserCredentials()
        {
            throw new NotImplementedException();
        }

        public string GetUsername()
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0, ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetIsFavourite(Guid id, bool isFavourite, string serverUrl)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartSyncAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateDb(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
