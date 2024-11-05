using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaServerGenreConnector
    {
        Task<Genre[]> GetAllGenresAsync();
        Task<Genre> GetGenreAsync(int genreId);
        Task<int> GetTotalGenreCountAsync();
    }
}
