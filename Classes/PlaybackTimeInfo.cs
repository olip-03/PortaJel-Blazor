using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class PlaybackTimeInfo
    {
        public long currentDuration = 0;
        public long fullDuration = 0;
        public Guid currentTrackGuid = Guid.Empty;

        public PlaybackTimeInfo(long setCurrentTime, long setFullTime, Guid currentTrackGuid)
        {
            currentDuration = setCurrentTime;
            fullDuration = setFullTime;
            this.currentTrackGuid = currentTrackGuid;
        }
    }
}
