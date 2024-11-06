using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;
using SQLite;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemGenreConnector : IMediaServerGenreConnector
{
    private SQLiteAsyncConnection _database = null;
    
    public FileSystemGenreConnector(SQLiteAsyncConnection database)
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
}