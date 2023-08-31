using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Album
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string artist { get; set; } = string.Empty;
        public string imageSrc { get; set; } = string.Empty;
        public string imageArtistSrc { get; set; } = string.Empty;

        public Song[] songs { get; set; } 
    }
}
