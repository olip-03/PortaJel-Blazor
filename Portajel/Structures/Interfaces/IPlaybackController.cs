using Portajel.Structures.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.Interfaces
{
    public interface IPlaybackController
    {
        /// <summary>
        /// Gets a boolean value indicating the current play state.
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// Gets a boolean value indicating the current shuffle mode.
        /// </summary>
        bool IsShuffle { get; }
        /// <summary>
        /// Gets the current repeat mode using a custom enum.
        /// </summary>
        PlaybackRepeatMode RepeatMode { get; }
        /// <summary>
        /// Gets the current playback position as a long integer.
        /// </summary>
        long PlaybackPosition { get; }
        /// <summary>
        /// Sets the play state of the playback controller.
        /// </summary>
        /// <param name="isPlaying">True to play, false to pause.</param>
        void SetPlay(bool isPlaying);
        /// <summary>
        /// Sets the shuffle mode of the playback controller.
        /// </summary>
        /// <param name="isShuffle">True to enable shuffle, false to disable.</param>
        void SetShuffle(bool isShuffle);
        /// <summary>
        /// Sets the repeat mode of the playback controller.
        /// </summary>
        /// <param name="repeatMode">The repeat mode to set.</param>
        void SetRepeatMode(PlaybackRepeatMode repeatMode);
        /// <summary>
        /// Seeks to a specific position in the current playback.
        /// </summary>
        /// <param name="seekPosition">The position to seek to, in milliseconds.</param>
        void SeekTo(long seekPosition);
    }
}
