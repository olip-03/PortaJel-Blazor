using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Playlist: BaseMusicItem
    {
        public Guid? Id => _playlistData.Id;
        public string Name => _playlistData.Name;
        public bool IsFavourite => _playlistData.IsFavourite;
        public string ImgSource => _playlistData.ImgSource;
        public string ImgBlurhash => _playlistData.ImgBlurhash;
        public string Path => _playlistData.Path;
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
