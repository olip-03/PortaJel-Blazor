using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService
    {
        public bool isPlaying = false;
        public int repeatMode = 0;
        public bool shuffleOn = false;
        public SongQueue songQueue = new();
        public SongQueue nextUpQueue = new();
        partial void Initalize();
        partial void Play();
        partial void Pause();
        partial void TogglePlay();
        partial void ToggleShuffle();
        partial void ToggleRepeat();
        partial void NextTrack();
        partial void PreviousTrack();
        partial void SeekTo(long position);
    }
}
