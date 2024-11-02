using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;
namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabasePlaylistConnector : IMediaServerPlaylistConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabasePlaylistConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public Task<Playlist[]> GetAllPlaylistsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Playlist> GetPlaylistAsync(int playlistId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalPlaylistCountAsync()
    {
        throw new NotImplementedException();
    }
}