using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Playlist: BaseMusicItem
    {
        public Song[] songs = new Song[0];
        public bool isFavourite = false;
        public static readonly Playlist Empty = new();    
        public Playlist() 
        {
            
        }
    }
}
