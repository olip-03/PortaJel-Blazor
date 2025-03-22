using Portajel.Connections.Database;

namespace Portajel.Connections.Data
{
    public class Genre : BaseMusicItem
    {
        public GenreData GetBase => _genreData;
        // public new Guid LocalId => _genreData.LocalId;
        public new Guid Id => _genreData.Id;
        public string ServerAddress => _genreData.ServerAddress;
        public new string Name => _genreData.Name;
        public new DateTimeOffset DateAdded => _genreData.DateAdded;
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
