using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Playlist: BaseMusicItem
    {
        public Song[] songs = new Song[0];
        public string path = string.Empty;
        public static readonly Playlist Empty = new();    
        public Playlist() 
        {

        }

        public List<ContextMenuItem> GetContextMenuItems()
        {
            contextMenuItems.Clear();

            if (isFavourite)
            {
                contextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Task(async () =>
                {
                    isFavourite = false;
                    await MauiProgram.api.SetFavourite(this, false);
                })));
            }
            else
            {
                contextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Task(async () =>
                {
                    isFavourite = true;
                    await MauiProgram.api.SetFavourite(this, true);
                })));
            }
            contextMenuItems.Add(new ContextMenuItem("Edit Playlist", "light_edit.png", new Task(async () =>
            {
                await MauiProgram.MainPage.NavigateToPlaylistEdit(this.id);
            })));
            contextMenuItems.Add(new ContextMenuItem("Download", "light_cloud_download.png", new Task(() =>
            {

            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Playlist", "light_playlist.png", new Task(() =>
            {

            })));
            contextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Task(() =>
            {

            })));
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
            })));

            return contextMenuItems;
        }
    }
}
