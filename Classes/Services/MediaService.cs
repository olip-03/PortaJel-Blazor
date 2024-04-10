using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes.Services
{
    public interface IMediaInterface
    {
        void Initalize();
        void Play();
        void Pause();
        void TogglePlay();
        void ToggleShuffle();
        void ToggleRepeat();
        void NextTrack();
        void PreviousTrack();
        void SeekToPosition(long position);
        void SeekToIndex(int index);
        void SetPlayingCollection(BaseMusicItem baseMusicItem, int fromIndex = 0);
        void AddSong(Song song);
        void AddSongs(Song[] songs);
        void RemoveSong(int index);
        Song[] GetQueue();
        int GetQueueIndex();
        bool GetIsPlaying();
        bool GetIsShuffling();
        int GetRepeatMode();
    }
}
