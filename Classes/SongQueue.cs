using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class SongQueue
    {
        private Queue<Song> songQueue = new Queue<Song>();
        private List<Song> dequeued = new List<Song>();
        public void QueueSong(Song _song)
        {
            Song? lastSong = songQueue.FirstOrDefault();
            songQueue.Enqueue(_song);

            if (lastSong != null)
            {
                if (lastSong != songQueue.FirstOrDefault())
                {
                    // Refresh UI
                }
            }
            else
            {
                // Refresh UI
            }
        }
        public void QueueRange(Song[] _songList)
        {
            foreach (var _song in _songList)
            {
                songQueue.Enqueue(_song);
            }
        }
        public Song DequeueSong()
        {
            Song dequeued = songQueue.Dequeue();
            return dequeued;
        }
        public void Clear()
        {
            songQueue.Clear();
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

}
