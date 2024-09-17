using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public abstract class BaseMusicItem
    {
        //public Guid id { get; set; } = Guid.Empty;
        //public string name { get; set; } = String.Empty;
        //public bool isFavourite { get; set; } = false;
        //public int playCount { get; set; } = 0;
        //public int dateAdded { get; set; } = 0;
        //public string serverAddress { get; set; } = String.Empty;
        //public MusicItemImage image { get; set; } = MusicItemImage.Empty;
        //public List<ContextMenuItem> contextMenuItems { get; set; } = new();
        public Album ToAlbum()
        {
            return (Album)this;
        }
        public Artist ToArtist()
        {
            return (Artist)this;
        }
        public Playlist ToPlaylist()
        {
            return (Playlist)this;
        }
        public Song ToSong()
        {
            return (Song)this;
        }
        public static bool IsNullOrEmpty(BaseMusicItem item)
        {
            if (item == null) return true;
            if (item is Album)
            {
                if(item == Album.Empty)return true;
            }
            else if (item is Song)
            {
                if (item == Song.Empty) return true;

            }
            else if (item is Artist)
            {
                if (item == Artist.Empty) return true;

            }
            else if (item is Playlist)
            {
                if (item == Playlist.Empty) return true;
            }
            return false;
        }
    }
}
