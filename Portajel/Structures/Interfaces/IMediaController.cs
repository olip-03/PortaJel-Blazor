using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.Interfaces
{
    public interface IMediaController
    {
        IPlaybackController Playback { get; }
        IQueueController Queue { get; }
        void Initialize();
        void Destroy();
    }
}
