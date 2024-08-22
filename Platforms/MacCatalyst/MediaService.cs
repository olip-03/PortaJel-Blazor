using PortaJel_Blazor.Classes.Interfaces;
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
        public void AddSong(Song song)
        {
            throw new NotImplementedException();
        }

        public void AddSongs(Song[] songs)
        {
            throw new NotImplementedException();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public Song GetCurrentlyPlaying()
        {
            throw new NotImplementedException();
        }

        public bool GetIsPlaying()
        {
            throw new NotImplementedException();
        }

        public bool GetIsShuffling()
        {
            throw new NotImplementedException();
        }

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            throw new NotImplementedException();
        }

        public BaseMusicItem GetPlayingCollection()
        {
            throw new NotImplementedException();
        }

        public SongGroupCollection GetQueue()
        {
            throw new NotImplementedException();
        }

        public int GetQueueIndex()
        {
            throw new NotImplementedException();
        }

        public int GetRepeatMode()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Initalize()
        {
            throw new NotImplementedException();
        }

        public void NextTrack()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void PreviousTrack()
        {
            throw new NotImplementedException();
        }

        public void RemoveSong(int index)
        {
            throw new NotImplementedException();
        }

        public void SeekToIndex(int index)
        {
            throw new NotImplementedException();
        }

        public void SeekToPosition(long position)
        {
            throw new NotImplementedException();
        }

        public void SetPlayAddonAction(Action addonAction)
        {
            throw new NotImplementedException();
        }

        public void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            throw new NotImplementedException();
        }

        public void TogglePlay()
        {
            throw new NotImplementedException();
        }

        public void ToggleRepeat()
        {
            throw new NotImplementedException();
        }

        public void ToggleShuffle()
        {
            throw new NotImplementedException();
        }
    }
}
