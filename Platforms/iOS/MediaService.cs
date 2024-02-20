using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// IOS Implementation
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService
    {
        public partial void Play()
        {

        }
        public partial void Pause()
        {

        }
        public partial void TogglePlay()
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }
        public partial void ToggleShuffle()
        {

        }
        public partial void ToggleRepeat()
        {

        }
        public partial void NextTrack()
        {

        }
        public partial void PreviousTrack()
        {

        }
    }
}
