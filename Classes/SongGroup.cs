using AngleSharp.Common;
using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Classes
{
    public class SongGroup : List<Song>
    {
        public string Name { get; private set; } = "New Song Group";
        public bool IsVisible { 
            get 
            {
                return (this.Count > 0);
            }
            private set { } 
        }
        public SongGroup(string? name, List<Song> songs) : base(songs)
        {
            if (name != null)
            {
                Name = name;
            }           
        }

        public SongGroup(string? name)
        {
            if (name != null)
            {
                Name = name;
            }
        }
    }
}
