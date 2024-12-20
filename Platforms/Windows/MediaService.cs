﻿using PortaJel_Blazor.Classes.Interfaces;
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
        async Task<bool> Initalize()
        {
            await Task.Delay(100);
            return true;
        }
        async Task<bool> IMediaInterface.Initalize()
        {
            await Task.Delay(100);
            return true;
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

        Song IMediaInterface.GetCurrentlyPlaying()
        {
            return GetCurrentlyPlaying();
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
            return null;
        }

        SongGroupCollection IMediaInterface.GetQueue()
        {
            return null;
        }

        public void SetPlayAddonAction(Action addonAction)
        {
            
        }

        public BaseMusicItem GetPlayingCollection()
        {
            return Album.Empty;
        }

        public void Destroy()
        {

        }
    }
}
