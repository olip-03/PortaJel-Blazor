using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemSongConnector : IMediaServerSongConnector
{
    public async Task<Song[]> GetAllSongsAsync(int limit = 50, int startIndex = 0, bool getFavourite = false,
        ItemSortBy setSortTypes = ItemSortBy.Album, SortOrder setSortOrder = SortOrder.Ascending,
        string serverUrl = "", CancellationToken cancellationToken = default)
    {
        // Implementation to fetch all songs
        return await Task.FromResult(new Song[0]);
    }
    
    public async Task<Song> GetSongAsync(Guid id, string serverUrl = "", CancellationToken cancellationToken = default)
    {
        // Implementation to fetch a specific song by its ID
        return await Task.FromResult(new Song());
    }
    
    public async Task<Song[]> GetSimilarSongsAsync(Guid id, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to fetch similar songs to the specified one
        return await Task.FromResult(new Song[0]);
    }
    
    public async Task<int> GetTotalSongCountAsync(bool getFavourite = false, string serverUrl = "",
        CancellationToken cancellationToken = default)
    {
        // Implementation to get the total count of songs
        return await Task.FromResult(0);
    }
}