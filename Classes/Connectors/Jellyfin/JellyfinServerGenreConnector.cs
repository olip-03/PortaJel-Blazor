using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.Jellyfin;

public class JellyfinServerGenreConnector(JellyfinApiClient api, JellyfinSdkSettings clientSettings, UserDto user) : IMediaServerGenreConnector
{
    public async Task<Genre[]> GetAllGenresAsync()
    {
        // Implementation to fetch all genres
        return await Task.FromResult(new Genre[0]);
    }

    public async Task<Genre> GetGenreAsync(int genreId)
    {
        // Implementation to fetch a specific genre by its ID
        return await Task.FromResult(new Genre());
    }

    public async Task<int> GetTotalGenreCountAsync()
    {
        // Implementation to get the total count of genres
        return await Task.FromResult(0);
    }
}
