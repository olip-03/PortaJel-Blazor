using Jellyfin.Sdk.Generated.Models;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Database;
using PortaJel_Blazor.Classes.Interfaces;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.Database;

public class DatabaseGenreConnector  : IMediaServerGenreConnector
{
    private readonly SQLiteAsyncConnection _database = null;

    public DatabaseGenreConnector(SQLiteAsyncConnection database)
    {
        _database = database;
    }
    
    public Task<Genre[]> GetAllGenresAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Genre> GetGenreAsync(int genreId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalGenreCountAsync()
    {
        throw new NotImplementedException();
    }
    public async Task<bool> AddRange(Genre[] genres, CancellationToken cancellationToken = default)
    {
        // foreach (var g in genres)
        // {
        //     await _database.InsertOrReplaceAsync(g.GetBase, genres.First().GetBase.GetType());
        //     if (cancellationToken.IsCancellationRequested)
        //     {
        //         return false;
        //     }
        // }
        return false;
    }
}