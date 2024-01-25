using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class MediaService
    {
        public SongQueue songQueue = new();
        public class SongQueue
        {
            private Queue<Song> songQueue = new Queue<Song>();

            public void QueueSong(Song _song)
            {
                Song? lastSong = songQueue.FirstOrDefault();
                songQueue.Enqueue(_song);

                if (lastSong != null)
                {
                    if(lastSong != songQueue.FirstOrDefault())
                    {
                        // Refresh UI
                    }
                }
                else
                {
                    // Refresh UI
                }
            }
            public void QueueSong(Song[] _songList)
            {
                bool refreshUi = false;
                foreach (var _song in _songList)
                {
                    Song? lastSong = songQueue.FirstOrDefault();
                    songQueue.Enqueue(_song);

                    if (lastSong != null)
                    {
                        if (lastSong != songQueue.FirstOrDefault())
                        {
                            refreshUi = true;
                            // Refresh UI
                        }
                    }
                    else
                    {
                        refreshUi = true;
                        // Refresh UI
                    }
                }
                // Refresh UI
            }
            public Song DequeueSong()
            {
                Song? lastSong = songQueue.FirstOrDefault();
                Song dequeued = songQueue.Dequeue();

                if (lastSong != null)
                {
                    if (lastSong != dequeued)
                    {
                        // Refresh UI
                    }
                }
                else
                {

                }

                return dequeued;
            }
            public void Clear()
            {
                songQueue.Clear();
                // Refresh UI
            }
            public int Count()
            {
                return songQueue.Count();
            }
            public Song ElementAt(int i)
            {
                return songQueue.ElementAt(i);
            }
            public Song? FirstOrDefault()
            {
                return songQueue.FirstOrDefault();
            }
            public Song[] GetQueue()
            {
                return songQueue.ToArray();
            }
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
        public void NextTrack()
        {

        }
        public void PreviousTrack()
        {

        }
    }
}
