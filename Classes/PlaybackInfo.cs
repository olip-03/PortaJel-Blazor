using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class PlaybackInfo
    {
        public long currentDuration = -1;
        public long fullDuration = -1;
        public Guid currentTrackGuid = Guid.Empty;
        public int playingIndex = -1;

        public PlaybackInfo(long setCurrentTime, long setFullTime, Guid currentTrackGuid, int setPlayingIndex)
        {
            this.currentDuration = setCurrentTime;
            this.fullDuration = setFullTime;
            this.currentTrackGuid = currentTrackGuid;
            this.playingIndex = setPlayingIndex;
        }
    }
}
