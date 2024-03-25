using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes.Services
{
    public partial class MediaService
    {
        public bool isPlaying = false;
        public int repeatMode = 0;
        public bool shuffleOn = false;
        public SongQueue songQueue = new();
        public SongQueue nextUpQueue = new();
        /// <summary>
        /// Bool represents true if the song came from the queue, false if it came from nextup
        /// </summary>
        public Song? previousSong { 
            get
            {
                Song? toReturn = null;
                if (songQueue.dequeuedList.Count > 0)
                {
                    // remove song from dequeue list 
                    toReturn = songQueue.dequeuedList.Last();
                    songQueue.dequeuedList.RemoveAt(songQueue.dequeuedList.Count - 1);
                    songQueue.QueueAtFront(toReturn);
                    // add song back to the front of queues
                }
                else if (nextUpQueue.dequeuedList.Count > 0)
                {
                    // remove song from dequeue list 
                    toReturn = nextUpQueue.dequeuedList.Last();
                    nextUpQueue.dequeuedList.RemoveAt(nextUpQueue.dequeuedList.Count - 1);
                    nextUpQueue.QueueAtFront(toReturn);
                }
                return toReturn;
            }
            private set { }
        }
        public Song? nextSong { 
            get
            {
                Song? toReturn = null;
                if (songQueue.Count() > 0)
                {
                    toReturn = songQueue.DequeueSong();
                }
                else if (nextUpQueue.Count() > 0)
                {
                    toReturn = nextUpQueue.DequeueSong();
                }
                return toReturn;
            }
            private set { }
        }
        public Song? PeekPreviousSong()
        {
            Song? toReturn = null;
            if (songQueue.dequeuedList.Count > 0)
            {
                toReturn = songQueue.dequeuedList.Last();

            }
            else if (nextUpQueue.dequeuedList.Count > 0)
            {
                // remove song from dequeue list 
                toReturn = nextUpQueue.dequeuedList.Last();
            }
            return toReturn;
        }
        public Song? PeekNextSong()
        {
            Song? toReturn = null;
            if (songQueue.Count() > 0)
            {
                toReturn = songQueue.PeekSong();
            }
            else if (nextUpQueue.Count() > 0)
            {
                toReturn = nextUpQueue.PeekSong();
            }
            return toReturn;
        }
        public partial void Initalize();
        public partial void Play();
        public partial void Pause();
        public partial void TogglePlay();
        public partial void ToggleShuffle();
        public partial void ToggleRepeat();
        public partial void NextTrack();
        public partial void PreviousTrack();
        public partial void SeekTo(long position);
    }
}
