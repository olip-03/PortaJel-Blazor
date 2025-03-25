using Portajel.Connections.Data;
using Jellyfin.Sdk.Generated.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Connections.Interfaces
{
    public interface IDbConnector
    {
        Dictionary<string, IDbItemConnector> GetDataConnectors();
        public Task<BaseMusicItem[]> SearchAsync(
            string searchTerm = "",
            int? limit = null,
            int startIndex = 0,
            ItemSortBy setSortTypes = ItemSortBy.Name,
            SortOrder setSortOrder = SortOrder.Ascending,
            CancellationToken cancellationToken = default);
    }
}
