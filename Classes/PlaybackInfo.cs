using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Classes
{
    public class PlaybackInfo
    {
        public long currentDuration = -1;
        public Song currentSong = Song.Empty;
        public int playingIndex = -1;
        public bool isPlaying = false;

        public PlaybackInfo(long setCurrentTime, Song currentSong, int setPlayingIndex, bool isPlaying)
        {
            this.currentDuration = setCurrentTime;
            this.currentSong = currentSong;
            this.playingIndex = setPlayingIndex;
            this.isPlaying = isPlaying;
        }
    }
}
