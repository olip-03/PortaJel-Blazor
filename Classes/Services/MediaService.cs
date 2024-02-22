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
        public SongQueue songQueue = new();
        public partial void Initalize();
        public partial void Play();
        public partial void Pause();
        public partial void TogglePlay();
        public partial void ToggleShuffle();
        public partial void ToggleRepeat();
        public partial void NextTrack();
        public partial void PreviousTrack();
    }
}
