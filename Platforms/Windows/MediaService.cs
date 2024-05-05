using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// IOS Implementation
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService : IMediaInterface
    {
        public void Initalize()
        {

        }
        public void Play()
        {

        }
        public void Pause()
        {

        }
        public void TogglePlay()
        {
        }
        public void ToggleShuffle()
        {

        }
        public void ToggleRepeat()
        {

        }
        public void NextTrack()
        {

        }
        public void PreviousTrack()
        {

        }
        public void SeekToPosition(long position)
        {

        }

        public void SeekToIndex(int index)
        {

        }

        public void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {

        }

        public void AddSong(Song song)
        {

        }

        public void RemoveSong(int index)
        {

        }

        public void AddSongs(Song[] songs)
        {
            
        }

        public Song[] GetQueue()
        {
            return Array.Empty<Song>();
        }

        public int GetQueueIndex()
        {
            return 0;
        }

        public bool GetIsPlaying()
        {
            return false;  
        }

        public bool GetIsShuffling()
        {
            return false;
        }

        public int GetRepeatMode()
        {
            return 0;
        }

        public Song GetCurrentlyPlaying()
        {
            return Song.Empty;
            //throw new NotImplementedException();
        }

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            throw new NotImplementedException();
        }
    }
}
