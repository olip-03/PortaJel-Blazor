namespace PortaJel_Blazor.Classes.Data
{
    public abstract class BaseMusicItem
    {
        public string ServerAddress { get; set; }
        public Guid LocalId { get; set; }
        public Guid Id { get; set; }
        public string ImgSource { get; set; }
        public string ImgBlurhash { get; set; }
        public string ImgBlurhashBase64 { get; set; }
        public string Name { get; set; }
        public bool IsFavourite { get; set; }
        public int PlayCount { get; set; }
        public DateTimeOffset? DateAdded { get; set; }
        public DateTimeOffset? DatePlayed { get; set; }
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
