﻿using PortaJel_Blazor.Data;
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
            throw new NotImplementedException();
        }

        public Song[] GetQueue()
        {
            throw new NotImplementedException();
        }

        public int GetQueueIndex()
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
            throw new NotImplementedException();
        }
    }
}
