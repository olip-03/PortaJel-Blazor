using Portajel.Structures.Interfaces;
using Portajel.Structures.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Droid.Playback
{
    public class PlaybackController : IPlaybackController
    {
        public bool IsPlaying => _isPlaying;
        public bool IsShuffle => _isShuffle;
        public PlaybackRepeatMode RepeatMode => _playbackRepeatMode;
        public long PlaybackPosition => _playbackPosition;

        private bool _isPlaying;
        private bool _isShuffle;
        private PlaybackRepeatMode _playbackRepeatMode;
        private long _playbackPosition;

        public void SeekTo(long seekPosition)
        {
            _playbackPosition = seekPosition;
        }

        public void SetPlay(bool isPlaying)
        {
            _isPlaying = isPlaying;
        }

        public void SetRepeatMode(PlaybackRepeatMode repeatMode)
        {
            _playbackRepeatMode = repeatMode;
        }

        public void SetShuffle(bool isShuffle)
        {
            _isShuffle = isShuffle;
        }
    }
}
