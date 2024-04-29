using PortaJel_Blazor.Data;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using Android.OS;

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    public class MediaServiceBinder : Binder
    {
        public AndroidMediaService Service { get; private set; }
        public MediaServiceBinder(AndroidMediaService service)
        {
            this.Service = service;
        }

        public bool Play()
        {
            return Service.Play();
        }
        public bool TogglePlay()
        {
            return Service.TogglePlay();
        }
        public bool Pause()
        {
            return Service.Pause();
        }
        public bool SeekToPosition(long position)
        {
            return Service.SeekToPosition(position);
        }
        public bool SeekToIndex(int index)
        {
            return Service.SeekToIndex(index);
        }
        public bool SetRepeat(MediaServiceRepeatMode repeatMode)
        {
            return Service.SetRepeat(repeatMode);
        }
        public bool ToggleRepeat()
        {
            return Service.ToggleRepeat();
        }
        public bool SetShuffle(bool isShullfing)
        {
            return Service.SetShuffle(isShullfing);
        }
        public bool GetShuffle()
        {
            return Service.GetShuffle();
        }
        public bool ToggleShuffle()
        {
            return Service.ToggleShuffle();
        }
        public bool Next()
        {
            return Service.Next();
        }
        public bool Previous()
        {
            return Service.Previous();
        }
        public bool SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0)
        {
            return Service.SetPlayingCollection(baseMusicItem, fromIndex);
        }
        public bool AddSong(Song song)
        {
            return Service.AddSong(song);
        }
        public bool AddSongs(Song[] songs)
        {
            return Service.AddSongs(songs);
        }
        public bool RemoveSong(int index)
        {
            return Service.RemoveSong(index);
        }
        public Song[] GetQueue()
        {
            return Service.GetQueue();
        }
        public int GetQueueIndex()
        {
            return Service.GetQueueIndex();
        }
        public bool GetIsPlaying()
        {
            return Service.GetIsPlaying();
        }
        public bool GetIsShuffling()
        {
            return Service.GetIsShuffling();
        }
        public int GetRepeatMode()
        {
            return Service.GetRepeatMode();
        }
        public Song GetCurrentlyPlaying()
        {
            return Service.GetCurrentlyPlaying();
        }
        public PlaybackTimeInfo? GetPlaybackTimeInfo()
        {
            return Service.GetPlaybackTimeInfo();
        }
    }

}
