using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Services.Database
{
    public class DroidDatabaseConnector : IDbConnector
    {
        public Dictionary<string, IDbItemConnector> GetDataConnectors()
        {
            throw new NotImplementedException();
        }

        public Task<BaseMusicItem[]> SearchAsync(string searchTerm = "", int? limit = null, int startIndex = 0, ItemSortBy setSortTypes = ItemSortBy.Name, SortOrder setSortOrder = SortOrder.Ascending, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
