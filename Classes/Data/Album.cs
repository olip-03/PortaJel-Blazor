using Jellyfin.Sdk;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PortaJel_Blazor.Classes.Database;

namespace PortaJel_Blazor.Data
{
    public class Album : BaseMusicItem, IComparable<Album>
    {
        public string ImgSource { get; set; } = "/images/emptyAlbum.png";
        public ArtistData[] Artists => _artistData;
        public string ArtistNames
        {
            get
            {
                if(Artists == null) { return String.Empty; }
                string data = string.Empty;
                for (int i = 0; i < Artists.Length; i++)
                {
                    data += Artists[i].Name;
                    if (i < Artists.Length - 1)
                    {
                        data += ", ";
                    }
                }
                return data;
            }
            private set
            {

            }
        }
        public Song[] songs { get; set; } = new Song[0];

        private SongData[] _songData;
        private ArtistData[] _artistData;

        public AlbumSortMethod sortMethod { get; set; } = AlbumSortMethod.name;
        public enum AlbumSortMethod
        {
            name,
            artist,
            id
        }
        public static readonly Album Empty = new();
        
        public string GetArtistName()
        {
            if(Artists == null)
            {
                return string.Empty;
            }
            string artistName = string.Join(", ", Artists.Select(artist => artist.name));
            return artistName;
        }

        public int CompareTo(Album? other)
        {
            int resolve = -1;
            switch (sortMethod)
            {
                case AlbumSortMethod.name:
                    if (name == null) { return -1; }
                    if (other.name == null) { return -1; }
                    resolve = string.Compare(name.ToString(), other.name.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                case AlbumSortMethod.artist:
                    if (GetArtistName() == null) { return -1; }
                    if (other.GetArtistName() == null) { return -1; }
                    resolve = string.Compare(GetArtistName().ToString(), other.GetArtistName().ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                case AlbumSortMethod.id:
                    if (id == System.Guid.Empty) { return -1; }
                    if (other.id == System.Guid.Empty) { return -1; }
                    resolve = string.Compare(id.ToString(), other.id.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    if (name == null) { return -1; }
                    if (other.name == null) { return -1; }
                    resolve = string.Compare(name.ToString(), other.name.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
            }
            return resolve;
        }
        
        public string imageAtResolution(int px)
        {
            string data = ImgSource + $"?fillHeight={px}&fillWidth={px}&quality=96";
            return data;
        }
        
        public static async Task<Album> Builder(AlbumData sqlalbum)
        {
            await Task.Delay(100);
            return new Album();
        }

        public List<ContextMenuItem> GetContextMenuItems()
        {
            contextMenuItems.Clear();
            
            if (this.isFavourite)
            {
                contextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Task(async () =>
                {
                    this.isFavourite = false;
                    await MauiProgram.api.SetFavourite(this.id, this.serverAddress, false);
                })));
            }
            else
            {
                contextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Task(async () =>
                {
                    this.isFavourite = true;
                    await MauiProgram.api.SetFavourite(this.id, this.serverAddress, true);
                })));
            }
            contextMenuItems.Add(new ContextMenuItem("Download", "light_cloud_download.png", new Task(() =>
            {

            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Playlist", "light_playlist.png", new Task(() =>
            {

            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Task(async () =>
            {
                Album FullAlbum = await MauiProgram.api.GetAlbumAsync(id);
                this.songs = FullAlbum.songs;

                MauiProgram.MediaService.AddSongs(FullAlbum.songs.ToArray());

                #if !WINDOWS
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                string text = $"{songs.Count()} songs added to queue.";
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                #endif
            })));
            contextMenuItems.Add(new ContextMenuItem("View Album", "light_album.png", new Task(async() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
                await MauiProgram.MainPage.AwaitContextMenuClose();
                MauiProgram.MainPage.ShowLoadingScreen(true);
                MauiProgram.WebView.NavigateAlbum(this.id);
            })));
            if(this.Artists.Count() > 0)
            {
                contextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Task(async () =>
                {
                    Artist? artist = this.Artists.FirstOrDefault();
                    if(artist != null)
                    {
                        MauiProgram.MainPage.CloseContextMenu();
                        await MauiProgram.MainPage.AwaitContextMenuClose();
                        MauiProgram.MainPage.ShowLoadingScreen(true);
                        MauiProgram.WebView.NavigateArtist(artist.id);
                    }
                })));
            }
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
            })));
            
            return contextMenuItems;
        }
    }
}
