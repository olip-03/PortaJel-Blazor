using Jellyfin.Sdk;
using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PortaJel_Blazor.Data
{
    public class Album : BaseMusicItem, IComparable<Album>
    {
        public string imageSrc { get; set; } = "/images/emptyAlbum.png";
        public Artist[] artists { get; set; } = new Artist[0];
        public string artistCongregate
        {
            get
            {
                if(artists == null) { return String.Empty; }
                string data = string.Empty;
                for (int i = 0; i < artists.Length; i++)
                {
                    data += artists[i].name;
                    if (i < artists.Length - 1)
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
        public AlbumSortMethod sortMethod { get; set; } = AlbumSortMethod.name;
        public string serverAddress { get; set; } = string.Empty;
        public enum AlbumSortMethod
        {
            name,
            artist,
            id
        }
        public static readonly Album Empty = new();
        public string GetArtistName()
        {
            if(artists == null)
            {
                return string.Empty;
            }
            string artistName = string.Join(", ", artists.Select(artist => artist.name));
            return artistName;
        }

        public int CompareTo(Album other)
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
                    if (id == Guid.Empty) { return -1; }
                    if (other.id == Guid.Empty) { return -1; }
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
            string data = imageSrc + $"?fillHeight={px}&fillWidth={px}&quality=96";
            return data;
        }
        public List<ContextMenuItem> GetContextMenuItems()
        {
            contextMenuItems.Clear();
            
            if (this.isFavourite)
            {
                contextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Task(async () =>
                {
                    this.isFavourite = false;
                    await MauiProgram.servers[0].FavouriteItem(this.id, false);
                })));
            }
            else
            {
                contextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Task(async () =>
                {
                    this.isFavourite = true;
                    await MauiProgram.servers[0].FavouriteItem(this.id, true);
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

                MauiProgram.mediaService.AddSongs(FullAlbum.songs.ToArray());

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
                MauiProgram.mainPage.CloseContextMenu();
                await MauiProgram.webView.FlagLoading();
                await MauiProgram.mainPage.AwaitContextMenuClose();
                MauiProgram.webView.NavigateAlbum(this.id);
            })));
            contextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Task(async() =>
            {
                MauiProgram.mainPage.CloseContextMenu();
                await MauiProgram.webView.FlagLoading();
                await MauiProgram.mainPage.AwaitContextMenuClose();
                MauiProgram.webView.NavigateArtist(this.artists.FirstOrDefault().id);
            })));
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.mainPage.CloseContextMenu();
            })));
            
            return contextMenuItems;
        }
    }
}
