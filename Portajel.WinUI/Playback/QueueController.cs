using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Structures.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.WinUI.Playback
{
    public class QueueController : IQueueController
    {
        public List<Song> PreviousQueue { get; set; }
        public KeyValuePair<MediaTypes, BaseMusicItem> CurrentSong => throw new NotImplementedException();
        public KeyValuePair<MediaTypes, BaseMusicItem> CurrentColleciton => throw new NotImplementedException();
        public List<Song> UpNextList { get; set; }

        public void AddSong(Song toAdd, int index)
        {
            throw new NotImplementedException();
        }

        public void AddSong(Song[] toAdd, int index)
        {
            throw new NotImplementedException();
        }

        public void ClearCollection(bool removeFromQueue)
        {
            throw new NotImplementedException();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(int fromIndex, int toIndex)
        {
            throw new NotImplementedException();
        }

        public void RemoveSong(int index)
        {
            throw new NotImplementedException();
        }

        public void SetCollection(Album collection, int fromIndex)
        {
            throw new NotImplementedException();
        }

        public void Skip()
        {
            throw new NotImplementedException();
        }
    }
}
