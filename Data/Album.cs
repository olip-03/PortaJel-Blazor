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
        public string imageSrc { get; set; } = string.Empty;
        public bool isSong { get; set; } = false;
        public Artist[] artists { get; set; }
        public Song[] songs { get; set; }

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
            // Compare albums based on their names
            return string.Compare(id.ToString(), other.id.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
