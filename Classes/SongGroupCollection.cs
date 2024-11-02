using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes
{
    public class SongGroupCollection : ObservableCollection<SongGroup>
    {
        public ObservableCollection<SongGroup> SongGroups => this.ToObservableCollection();
        public ObservableCollection<Song> AllSongs => this.SelectMany(group => group).ToObservableCollection();
        public int QueueStartIndex { get; set; } = -1;
        public int QueueCount { get; set; } = -1;
        public int GetTotalItems()
        {
            return this.SelectMany(group => group).Count();
        }
        public ObservableCollection<SongGroup> ToObservableCollection(SongGroupCollection collection)
        {
            return new ObservableCollection<SongGroup>(collection);
        }
    }

}
