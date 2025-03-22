using Portajel.Structures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Droid.Playback
{
    public class MediaInterface : IMediaController
    {
        public IPlaybackController Playback => new PlaybackController();
        public IQueueController Queue => new QueueController();

        public void Destroy()
        {
            
        }

        public void Initialize()
        {
            
        }
    }
}
