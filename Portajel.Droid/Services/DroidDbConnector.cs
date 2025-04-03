using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Interfaces;
using PortaJel.Droid.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Services
{
    public class DroidDbConnector: IDbConnector
    {
        private DroidServiceController _serviceConnection = null!;
        public Dictionary<string, IDbItemConnector> GetDataConnectors() => 
            _serviceConnection.AppServiceConnection.Binder?.Database != null ? 
            _serviceConnection.AppServiceConnection.Binder.Database.GetDataConnectors() : 
            throw new Exception("Cannot retrieve value without Binder.");
        public Task<BaseMusicItem[]> SearchAsync(
            string searchTerm = "", 
            int? limit = null, 
            int startIndex = 0, 
            ItemSortBy setSortTypes = ItemSortBy.Name, 
            SortOrder setSortOrder = SortOrder.Ascending, 
            CancellationToken cancellationToken = default) => 
            _serviceConnection.AppServiceConnection.Binder?.Database != null ?
            _serviceConnection.AppServiceConnection.Binder.Database.SearchAsync(
                searchTerm, 
                limit, 
                startIndex, 
                setSortTypes, 
                setSortOrder, 
                cancellationToken) :
            throw new Exception("Cannot retrieve value without Binder.");
        public DroidDbConnector(DroidServiceController serviceConnection)
        {
            _serviceConnection = serviceConnection;
        }
    }
}
