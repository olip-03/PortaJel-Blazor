using Portajel.Structures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Droid.Playback
{
    public class MediaController : IMediaController
    {
        public IPlaybackController Playback => _playbackController;
        public IQueueController Queue => _queueController;

        private PlaybackController _playbackController = new();
        private QueueController _queueController = new();

        public void Destroy()
        {
            
        }

        public void Initialize()
        {
            
        }
    }
}
