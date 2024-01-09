using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Song
    {
        public string id { get; set; }
        public string name { get; set; }
        public string artist { get; set; }
        public int disk { get; set; }
        public string imageSrc { get;set; }
        public Album album { get; set; }
    }
}
