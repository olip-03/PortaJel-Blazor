namespace PortaJel_Blazor.Classes.Data
{
    public abstract class BaseMusicItem
    {
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
