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
        private List<KeyValuePair<bool, Song>> dequeuedSongs = new();
        public Song? previousSong { 
            get
            {
                Song? toReturn = null;
                if (dequeuedSongs.Count > 0)
                {
                    if(dequeuedSongs.Last().Key == true)
                    {
                        // remove song from dequeue list 
                        toReturn = dequeuedSongs.Last().Value;
                        songQueue.dequeuedList.RemoveAt(songQueue.dequeuedList.Count - 1);
                        dequeuedSongs.RemoveAt(dequeuedSongs.Count() - 1);
                        songQueue.QueueAtFront(toReturn);
                        // add song back to the front of queues 
                    }
                }
                else if (nextUpQueue.dequeuedList.Count > 0)
                {
                    // remove song from dequeue list 
                    toReturn = nextUpQueue.dequeuedList.Last();
                    nextUpQueue.dequeuedList.RemoveAt(nextUpQueue.dequeuedList.Count - 1);
                    dequeuedSongs.RemoveAt(dequeuedSongs.Count() - 1);
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
                if (songQueue.dequeuedList.Count > 0)
                {
                    toReturn = songQueue.DequeueSong();
                    dequeuedSongs.Add(new KeyValuePair<bool, Song>(true, toReturn));
                }
                else if (nextUpQueue.dequeuedList.Count > 0)
                {
                    toReturn = nextUpQueue.DequeueSong();
                    dequeuedSongs.Add(new KeyValuePair<bool, Song>(false, toReturn));
                }
                return toReturn;
            }
            private set { }
        }
        public partial void Initalize();
        public partial void Play();
        public partial void Pause();
        public partial void TogglePlay();
        public partial void ToggleShuffle();
        public partial void ToggleRepeat();
        public partial void NextTrack();
        public partial void PreviousTrack();
    }
}
