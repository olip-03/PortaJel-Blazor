﻿using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Services;
using Android.OS;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    public class MediaServiceBinder : Binder
    {
        public BaseMusicItem? PlayingCollection { get; set; } = null;
        public int PlayingCollecionFromIndex { get; set; } = -1;
        public Song? AddToQueue { get; set; } = null;
        public AndroidMediaService Service { get; private set; }
        public MediaServiceBinder(AndroidMediaService service)
        {
            this.Service = service;
        }

        public void Initalize()
        {
            Service.Initalize();
        }
        public void Destroy()
        {
            Service.Destroy();
        }
        public bool Play()
        {
            return Service.Play();
        }
        public bool TogglePlay()
        {
            bool? playState = Service.TogglePlay();
            if(playState != null)
            {
                return playState.Value;
            }
            return false;
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
        public BaseMusicItem GetPlayingCollection()
        {
            return Service.GetPlayingCollection();
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
        public SongGroupCollection GetQueue()
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
        public PlaybackInfo? GetPlaybackTimeInfo()
        {
            return Service.GetPlaybackTimeInfo();
        }
    }

}
