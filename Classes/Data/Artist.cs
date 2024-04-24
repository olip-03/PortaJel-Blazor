using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Artist : BaseMusicItem
    {
        /// <summary>
        /// Boolean variable to determin if the information in this class is not complete
        /// </summary>
        public bool isPartial { get; set; } = false;
        public string description { get; set; } = String.Empty;
        public string imgSrc { get; set; } = String.Empty;
        public string backgroundImgSrc { get; set; } = String.Empty;
        public string logoImgSrc { get; set; } = String.Empty;
        public MusicItemImage backgroundImage { get; set; } = new();
        public MusicItemImage logoImage { get; set; } = new();
        public Album[] artistAlbums { get; set; } = new Album[0];

        public static Artist Empty = new Artist(); 
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
            contextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Task(async() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
                await MauiProgram.MainPage.AwaitContextMenuClose();
                MauiProgram.MainPage.ShowLoadingScreen(true);
                MauiProgram.WebView.NavigateArtist(this.id);
            })));
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(async() =>
            {
                MauiProgram.MainPage.CloseContextMenu();
            })));

            return contextMenuItems;
        }
    }
}
