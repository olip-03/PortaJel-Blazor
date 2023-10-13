using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Artist
    {
        public Guid id { get; set; } = Guid.Empty;
        public string name { get; set; } = string.Empty;
        public string imgSrc { get; set; } = string.Empty;
        public string backgroundImgSrc { get; set; } = string.Empty;
        public string logoImgSrc { get; set; } = string.Empty;

        public static Artist Empty = new Artist();
    }
}
