using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    public class Genre : BaseMusicItem
    {
        public Guid Id => _genreData.Id;
        public string ServerAddress => _genreData.ServerAddress;
        public string Name => _genreData.Name;
        public DateTimeOffset DateAdded => _genreData.DateAdded;
        public Guid[] AlbumIds => _genreData.GetAlbumIds();
        private readonly GenreData _genreData ;

        public Genre()
        {
            
        }
        public Genre(GenreData genreData)
        {
            _genreData = genreData;
        }
    }
}
