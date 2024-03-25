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
        public List<Song> dequeuedList = new List<Song>();
        public void QueueSong(Song _song)
        {
            Song? lastSong = songQueue.FirstOrDefault();
            songQueue.Enqueue(_song);
        }
        public void QueueAtFront(Song _song)
        {
            List<Song> toSet = songQueue.ToList();
            toSet.Insert(0, _song);

            songQueue.Clear();
            foreach (Song item in toSet)
            {
                songQueue.Enqueue(item);
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
            dequeuedList.Add(dequeued);
            return dequeued;
        }
        public Song PeekSong()
        {
            return songQueue.Peek();
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
