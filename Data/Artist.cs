﻿using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Artist : BaseMusicItem
    {
        public string description { get; set; } = String.Empty;
        public string imgSrc { get; set; } = String.Empty;
        public string backgroundImgSrc { get; set; } = String.Empty;
        public string logoImgSrc { get; set; } = String.Empty;
        public Album[] artistAlbums { get; set; } = new Album[0];

        public static Artist Empty = new Artist(); 

        public string imageAtResolution(int px)
        {
            return imgSrc += $"&fillHeight={px}&fillWidth={px}&quality=96";
        }
        public string backgroundImgAtResolution(int px)
        {
            return backgroundImgSrc += $"&fillHeight={px}&fillWidth={px}&quality=96";
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
            contextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Task(() =>
            {
                MauiProgram.mainLayout.NavigateArtist(this.id);
            })));
            contextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
            {
                MauiProgram.mainPage.CloseContextMenu();
            })));

            return contextMenuItems;
        }
    }
}
