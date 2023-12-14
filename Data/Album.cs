using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Album : IComparable<Album>
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string imageSrc { get; set; } = "/images/emptyAlbum.png";
        public string lowResImageSrc { get; set; } = "/images/emptyAlbum.png";
        public bool isSong { get; set; } = false;
        public bool isArtist { get; set; } = false;
        public Artist[] artists { get; set; }
        public Song[] songs { get; set; }
        public AlbumSortMethod sortMethod { get; set; } = AlbumSortMethod.name;
        public string serverAddress { get; set; } = string.Empty;
        public enum AlbumSortMethod
        {
            name,
            artist,
            id
        }
        
        public string GetArtistName()
        {
            if(artists == null)
            {
                return string.Empty;
            }
            string artistName = string.Join(", ", artists.Select(artist => artist.name));
            return artistName;
        }

        public int CompareTo(Album other)
        {
            int resolve = -1;
            switch (sortMethod)
            {
                case AlbumSortMethod.name:
                    if (name == null) { return -1; }
                    if (other.name == null) { return -1; }
                    resolve = string.Compare(name.ToString(), other.name.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                case AlbumSortMethod.artist:
                    if (GetArtistName() == null) { return -1; }
                    if (other.GetArtistName() == null) { return -1; }
                    resolve = string.Compare(GetArtistName().ToString(), other.GetArtistName().ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                case AlbumSortMethod.id:
                    if (id == Guid.Empty) { return -1; }
                    if (other.id == Guid.Empty) { return -1; }
                    resolve = string.Compare(id.ToString(), other.id.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    if (name == null) { return -1; }
                    if (other.name == null) { return -1; }
                    resolve = string.Compare(name.ToString(), other.name.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
            }
            return resolve;
        }
        public string imageAtResolution(int px)
        {
            string data = imageSrc + $"?fillHeight={px}&fillWidth={px}&quality=96";
            return data;
        }
    }
}
