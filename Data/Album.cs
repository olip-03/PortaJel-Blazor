﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Album
    {
        public string id { get; set; }
        public string name { get; set; }
        public string artist { get; set; }
        public string imageSrc { get; set; }
        public Song[] songs { get; set; } 
    }
}
