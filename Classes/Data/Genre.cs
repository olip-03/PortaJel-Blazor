using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    public class Genre : BaseMusicItem
    {
        public GenreData GetBase => _genreData;
        public Guid[] AlbumIds => _genreData.GetAlbumIds();
        private readonly GenreData _genreData ;
        public Genre()
        {
            
        }
        public Genre(GenreData genreData)
        {
            _genreData = genreData;
            Id = _genreData.Id;
            Name = _genreData.Name;
            DateAdded = _genreData.DateAdded;
            ServerAddress = _genreData.ServerAddress;
        }
    }
}
