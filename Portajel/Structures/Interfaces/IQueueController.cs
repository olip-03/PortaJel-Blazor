using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;

namespace Portajel.Structures.Interfaces
{
    public interface IQueueController
    {
        // Get Current Song
        KeyValuePair<MediaTypes, BaseMusicItem> CurrentSong { get; }
        // Get Current Collection
        KeyValuePair<MediaTypes, BaseMusicItem> CurrentColleciton { get; }
        // Function to skip the song, to the next
        void Skip();
        // Function to skip back to the prior song
        void Previous();
        // Function that allows you to add a song to the queue
        void AddSong(Song toAdd, int index);
        // Function that allows you to add several songs to the queue
        void AddSong(Song[] toAdd, int index);
        // Function to remove songs
        void RemoveSong(int index);
        // Function to remove several songs
        void RemoveRange(int fromIndex, int toIndex);
        // Function to set the playing collection
        void SetCollection(Album collection, int fromIndex);
        // Removes the current playing collection. If removeFromQueue is true
        // songs from collection are removed. If false, tracks up next in the playing 
        // collection are first up in queue.
        void ClearCollection(bool removeFromQueue);
    }
}
