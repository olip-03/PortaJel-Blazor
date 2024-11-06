using PortaJel_Blazor.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortaJel_Blazor.Classes.Data;

// IOS Implementation
namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService : IMediaInterface
    {
        BaseMusicItem IMediaInterface.GetPlayingCollection()
        {
            return GetPlayingCollection();
        }

        void IMediaInterface.AddSong(Song song)
        {
            AddSong(song);
        }

        void IMediaInterface.AddSongs(Song[] songs)
        {
            AddSongs(songs);
        }

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

        public int GetRepeatMode()
        {
            return 0;
        }

        Song IMediaInterface.GetCurrentlyPlaying()
        {
            return GetCurrentlyPlaying();
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
        

        public Task<bool> Initalize()
        {
            return new Task<bool>(() => true);
        }

        public void NextTrack()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Play()
        {
            
        }

        public void PreviousTrack()
        {
            
        }

        public void RemoveSong(int index)
        {
            
        }

        public void SeekToIndex(int index)
        {
            
        }

        public void SeekToPosition(long position)
        {
            
        }

        public void SetPlayAddonAction(Action addonAction)
        {
            
        }

        public void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            
        }

        public void TogglePlay()
        {
            
        }

        public void ToggleRepeat()
        {
            
        }

        public void ToggleShuffle()
        {
            
        }
    }
}
