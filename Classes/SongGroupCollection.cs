using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Data;
using System.Collections.ObjectModel;
namespace PortaJel_Blazor.Classes
{
    public class SongGroupCollection : ObservableCollection<SongGroup>
    {
        public ObservableCollection<SongGroup> SongGroups => this.ToObservableCollection();
        public ObservableCollection<Song> AllSongs => this.SelectMany(group => group).ToObservableCollection();
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
