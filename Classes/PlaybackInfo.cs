using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes
{
    public class PlaybackInfo
    {
        public TimeSpan Duration = new();
        public TimeSpan CurrentDuration = new();
        public Song CurrentSong = Song.Empty;
        public int PlayingIndex = -1;
        public bool IsBuffering = false;
        public bool IsPlaying = false;

        public PlaybackInfo(TimeSpan setDuration, TimeSpan setCurrentDuration, Song currentSong, int setPlayingIndex, bool isBuffering, bool isPlaying)
        {
            this.Duration = setDuration;
            this.CurrentDuration = setCurrentDuration;
            this.CurrentSong = currentSong;
            this.PlayingIndex = setPlayingIndex;
            this.IsBuffering = isBuffering;
            this.IsPlaying = isPlaying;
        }
    }
}
