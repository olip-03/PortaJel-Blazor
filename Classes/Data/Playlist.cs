using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Classes.Data
{
    public class Playlist: BaseMusicItem
    {
        public PlaylistData GetBase => _playlistData;
        public new Guid Id => _playlistData.Id;
        public string Name => _playlistData.Name;
        public bool IsFavourite => _playlistData.IsFavourite;
        public new string ImgSource => _playlistData.ImgSource;
        public new string ImgBlurhash => _playlistData.ImgBlurhash;
        public string Path => _playlistData.Path;
        public string ServerAddress => _playlistData.ServerAddress;
        public bool IsPartial { get; set; } = true;

        public Guid[] SongIds => _playlistData.GetSongIds();
        public SongData[] Songs => _songData;

        private PlaylistData _playlistData;
        private SongData[] _songData;

        public static readonly Playlist Empty = new();    
        public Playlist() 
        {
            _playlistData = new();
            _songData = [];
        }
        public Playlist(PlaylistData playlistData)
        {
            _playlistData = playlistData;
            _songData = [];
        }
        public Playlist(PlaylistData playlistData, SongData[] songData)
        {
            _playlistData = playlistData;
            _songData = songData;
            IsPartial = false;
        }
        public void SetIsFavourite(bool state)
        {
            _playlistData.IsFavourite = state;
        }

        //public List<ContextMenuItem> GetContextMenuItems()
        //{
        //    contextMenuItems.Clear();

        //    if (isFavourite)
        //    {
        //        contextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Task(async () =>
        //        {
        //            isFavourite = false;
        //            await MauiProgram.api.SetFavourite(this.id, this.serverAddress, false);
        //        })));
        //    }
        //    else
        //    {
        //        contextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Task(async () =>
        //        {
        //            isFavourite = true;
        //            await MauiProgram.api.SetFavourite(this.id, this.serverAddress, true);
        //        })));
        //    }
        //    contextMenuItems.Add(new ContextMenuItem("Edit Playlist", "light_edit.png", new Task(async () =>
        //    {
        //        await MauiProgram.MainPage.NavigateToPlaylistEdit(this.id);
        //    })));
        //    contextMenuItems.Add(new ContextMenuItem("Download", "light_cloud_download.png", new Task(() =>
        //    {

        //    })));
        //    contextMenuItems.Add(new ContextMenuItem("Add To Playlist", "light_playlist.png", new Task(() =>
        //    {

        //    })));
        //    contextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Task(() =>
        //    {

        //    })));
        //    contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
        //    {
        //        MauiProgram.MainPage.CloseContextMenu();
        //    })));

        //    return contextMenuItems;
        //}
    }
}
