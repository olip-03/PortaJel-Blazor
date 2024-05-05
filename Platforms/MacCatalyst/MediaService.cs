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

        public void AddSongs(Song[] songs)
        {

        }

        public void RemoveSong(int index)
        {

        }

        void IMediaInterface.Initalize()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.Play()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.Pause()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.TogglePlay()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.ToggleShuffle()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.ToggleRepeat()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.NextTrack()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.PreviousTrack()
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.SeekToPosition(long position)
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.SeekToIndex(int index)
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex)
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.AddSong(Song song)
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.AddSongs(Song[] songs)
        {
            throw new NotImplementedException();
        }

        void IMediaInterface.RemoveSong(int index)
        {
            throw new NotImplementedException();
        }

        Song[] IMediaInterface.GetQueue()
        {
            throw new NotImplementedException();
        }

        int IMediaInterface.GetQueueIndex()
        {
            throw new NotImplementedException();
        }

        bool IMediaInterface.GetIsPlaying()
        {
            throw new NotImplementedException();
        }

        bool IMediaInterface.GetIsShuffling()
        {
            throw new NotImplementedException();
        }

        int IMediaInterface.GetRepeatMode()
        {
            throw new NotImplementedException();
        }

        public Song GetCurrentlyPlaying()
        {
            throw new NotImplementedException();
        }

        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            throw new NotImplementedException();
        }
    }
}
