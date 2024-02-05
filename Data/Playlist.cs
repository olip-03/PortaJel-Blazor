using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Playlist: BaseMusicItem
    {
        public PlaylistSong[] songs = new PlaylistSong[0];
        public bool isFavourite = false;
        public static readonly Playlist Empty = new();    
        public Playlist() 
        {
            
        }
    }
}
