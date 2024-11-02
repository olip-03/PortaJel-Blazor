using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin;

public class JellyfinServerSongConnector : IMediaServerSongConnector
{
    public async Task<Song[]> GetAllSongsAsync()
    {
        // Implementation to fetch all songs
        return await Task.FromResult(new Song[0]);
    }
    
    public async Task<Song> GetSongAsync(int songId)
    {
        // Implementation to fetch a specific song by its ID
        return await Task.FromResult(new Song());
    }
    
    public async Task<Song[]> GetSimilarSongsAsync(int songId)
    {
        // Implementation to fetch similar songs to the specified one
        return await Task.FromResult(new Song[0]);
    }
    
    public async Task<int> GetTotalSongCountAsync()
    {
        // Implementation to get the total count of songs
        return await Task.FromResult(0);
    }
}