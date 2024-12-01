using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces
{
    public interface IMediaInterface
    {
        Task<bool> Initalize();
        void Destroy();
        void Play();
        void SetPlayAddonAction(Action addonAction);
        void Pause();
        void TogglePlay();
        void ToggleShuffle();
        void ToggleRepeat();
        void NextTrack();
        void PreviousTrack();
        void SeekToPosition(long position);
        void SeekToIndex(int index);
        void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0);
        BaseMusicItem GetPlayingCollection();
        void AddSong(Song song);
        void AddSongs(Song[] songs);
        void RemoveSong(int index);
        SongGroupCollection GetQueue();
        Song GetCurrentlyPlaying();
        public PlaybackInfo GetPlaybackTimeInfo();
        int GetQueueIndex();
        bool GetIsPlaying();
        bool GetIsShuffling();
        int GetRepeatMode();
    }
}
