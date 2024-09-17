using AngleSharp.Io;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Shared.Xaml;
using System.Diagnostics;

namespace PortaJel_Blazor.Shared;
public partial class ContextMenu : ContentView
{
    public bool isOpen { get; private set; } = false;
    public bool isSecondaryPageOpen { get; private set; } = false;

    private readonly IPopupService popupService;

    public ContextMenuViewModel ViewModel { get; set; } = new();
    private string fullImgUrl = string.Empty;
    private List<string> loadedImages = new();

    public ContextMenu(BaseMusicItem? baseMusicItem = null,
        string? blurBase64 = null,
        int? opacity = 100,
        bool? isCircle = false)
    {
        InitializeComponent();

        UpdateData(baseMusicItem, blurBase64, opacity, isCircle);
    }

    public ContextMenu()
    {
        InitializeComponent();

        ViewModel.HeaderHeightValue = MauiProgram.SystemHeaderHeight;
        Container.BindingContext = ViewModel;
    }

    public void UpdateData(BaseMusicItem? baseMusicItem = null,
        string? blurBase64 = null,
        int? opacity = 100,
        bool? isCircle = false)
    {
        if (ViewModel.ContextMenuItems == null) ViewModel.ContextMenuItems = new();
        if (baseMusicItem != null)
        {
            ViewModel.HeaderHeightValue = MauiProgram.SystemHeaderHeight;

            ViewModel.ContextMenuItems.Clear();

            /// #####################################################
            /// CREATE CONTEXT MENU ITEMS FOR ALBUM HERE
            /// #####################################################
            if (baseMusicItem is Album)
            {
                Album album = (Album)baseMusicItem;
                ViewModel.ContextMenuMainText = album.Name;
                fullImgUrl = album.ImgSource;

                if (album.IsFavourite)
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Action(async () =>
                    {
                        album.SetIsFavourite(false);
                        await MauiProgram.api.SetFavourite(album.Id, album.ServerAddress, false);
                    })));
                }
                else
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Action(async () =>
                    {
                        album.SetIsFavourite(true);
                        await MauiProgram.api.SetFavourite(album.Id, album.ServerAddress, true);
                    })));
                }
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Album Artist", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateArtist(album.ArtistIds.First());
                })));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Action(() =>
                {
                    // TODO: Add queue functionality 
                })));
                ViewModel.ContextMenuSubText = "Album • " + album.ArtistNames;
            }
            /// #####################################################
            /// CREATE CONTEXT MENU ITEMS FOR SONG HERE
            /// #####################################################
            else if (baseMusicItem is Song)
            {
                Song song = (Song)baseMusicItem;
                ViewModel.ContextMenuMainText = song.Name;
                fullImgUrl = song.ImgSource;

                if (song.IsFavourite)
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Action(async () =>
                    {
                        await MauiProgram.MainPage.MainContextMenu.Close();

                        song.SetIsFavourite(false);
                        await MauiProgram.api.SetFavourite((Guid)song.Id, song.ServerAddress, false);
                    })));
                }
                else
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Action(async () =>
                    {
                        await MauiProgram.MainPage.MainContextMenu.Close();

                        song.SetIsFavourite(true);
                        await MauiProgram.api.SetFavourite((Guid)song.Id, song.ServerAddress, true);
                    })));
                }
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Song Artist", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateArtist(song.ArtistIds.First());
                })));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Song Album", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateAlbum(song.AlbumId);
                })));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Action(() =>
                {
                    // TODO: Add queue functionality 
                })));
                ViewModel.ContextMenuSubText = "Song • " + song.ArtistNames;                                                    
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateArtist(song.ArtistIds.First());
                })));

                ViewModel.ContextMenuSubText = "Artist";
            }

            /// /// #####################################################
            /// CREATE CONTEXT MENU ITEMS FOR ARTISTS HERE
            /// #####################################################
            else if (baseMusicItem is Artist)
            {
                Artist artist = (Artist)baseMusicItem;
                ViewModel.ContextMenuMainText = artist.Name;
                fullImgUrl = artist.ImgSource;

                if (artist.IsFavourite)
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Action(async () =>
                    {
                        await MauiProgram.MainPage.MainContextMenu.Close();

                        artist.SetIsFavourite(false);
                        await MauiProgram.api.SetFavourite((Guid)artist.Id, artist.ServerAddress, false);
                    })));
                }
                else
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Action(async () =>
                    {
                        await MauiProgram.MainPage.MainContextMenu.Close();

                        artist.SetIsFavourite(true);
                        await MauiProgram.api.SetFavourite((Guid)artist.Id, artist.ServerAddress, true);
                    })));
                }
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Song Artist", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateArtist(artist.Id);
                })));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Action(() =>
                {
                    // TODO: Add queue functionality 
                })));
                ViewModel.ContextMenuSubText = "Artist";
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("View Artist", "light_artist.png", new Action(async () =>
                {
                    MauiProgram.MainPage.ShowLoadingScreen(true);

                    await MauiProgram.MainPage.MainContextMenu.Close();
                    MauiProgram.WebView.NavigateArtist(artist.Id);
                })));

                ViewModel.ContextMenuSubText = "Artist";
            }
            /// #####################################################
            /// CREATE CONTEXT MENU ITEMS FOR PLAYLIST HERE
            /// #####################################################
            else if (baseMusicItem is Playlist)
            {
                Playlist playlist = (Playlist)baseMusicItem;
                ViewModel.ContextMenuMainText = playlist.Name;
                fullImgUrl = playlist.ImgSource;

                if (playlist.IsFavourite)
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Remove From Favourites", "light_heart.png", new Action(async () =>
                    {
                        playlist.SetIsFavourite(false);
                        await MauiProgram.api.SetFavourite(playlist.Id, playlist.ServerAddress, false);
                    })));
                }
                else
                {
                    ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Favourites", "light_heart.png", new Action(async () =>
                    {
                        playlist.SetIsFavourite(true);
                        await MauiProgram.api.SetFavourite(playlist.Id, playlist.ServerAddress, true);
                    })));
                }                                                                 
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Edit Playlist", "light_edit.png", new Action(async () =>
                {
                    await MauiProgram.MainPage.NavigateToPlaylistEdit(playlist.Id);
                })));
                //ViewModel.ContextMenuItems.Add(new ContextMenuItem("Download", "light_cloud_download.png", new Action(() =>
                //{

                //})));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Playlist", "light_playlist.png", new Action(async () =>
                {
                    await ShowSecondarySelection();
                    UpdateSecondaryMenuData();
                })));
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Action(() =>
                {

                })));

                ViewModel.ContextMenuSubText = "Playlist";
            }

            ViewModel.ContextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Action(async () =>
            {
                await this.Close();
            })));

            Opacity = opacity ?? 100;
            IsVisible = true;

            ImageContainer.IsVisible = true;

            if (blurBase64 != null)
            {
                var imageBytes = Convert.FromBase64String(blurBase64);
                MemoryStream imageDecodeStream = new(imageBytes);
                this.ImageContainer_BackgroundImg.Source = ImageSource.FromStream(() => imageDecodeStream);
            }

            if (isCircle == true)
            {
                ImageContainer_Border.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(150, 150, 150, 150)
                };
            }
            else
            {
                ImageContainer_Border.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                };
            }
        }
    }

    /// <summary>
    /// Updates the data in the Context Menu with multiple items.
    /// </summary>
    /// <param name="songs"></param>
    /// <param name="blurBase64"></param>
    /// <param name="opacity"></param>
    public void UpdateData(Song[] songs, string[] blurBase64, int? opacity = 100)
    {
        if (ViewModel.ContextMenuItems == null) ViewModel.ContextMenuItems = new();

        ViewModel.HeaderHeightValue = MauiProgram.SystemHeaderHeight;
        ViewModel.ContextMenuItems.Clear();
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Favourite All", "light_heart.png", new Action(async () =>
        {
            await this.Close();
        })));
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Unfavourite all", "light_heart.png", new Action(async () =>
        {
            await this.Close();
        })));
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Download All", "light_cloud_download.png", new Action(async () =>
        {
            await this.Close();
        })));
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add to Playlist", "light_playlist.png", new Action(async () =>
        {
            MauiProgram.MainPage.ShowLoadingScreen(true);
            await this.Close();
            MauiProgram.WebView.NavigateToPlaylistAdd(songs.Select(s => s.Id).ToArray());
        })));
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Add To Queue", "light_queue.png", new Action(async () =>
        {
            await this.Close();
        })));
        ViewModel.ContextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Action(async () =>
        {
            await this.Close();
        })));
        ViewModel.ContextMenuMainText = $"{songs.Length} songs selected.";
        ViewModel.ContextMenuSubText = "";
    }

    private async void UpdateSecondaryMenuData()
    {
        if (ViewModel.SecondaryMenuItems == null) ViewModel.SecondaryMenuItems = new();
        ViewModel.SecondaryMenuItems.Clear();

        Playlist[] playlists = await MauiProgram.api.GetAllPlaylistsAsync();
        foreach (Playlist playlist in playlists)
        {
            ViewModel.SecondaryMenuItems.Add((new ContextMenuItem(playlist.Name, playlist.ImgSource, new Action(async () =>
            {
                await this.Close();
            }), 64, 80)));
        }
        ViewModel.SecondaryMenuItems.Add(new ContextMenuItem("Back", "light_back.png", new Action(async () =>
        {
            ItemList.ItemsSource = ViewModel.ContextMenuItems;
            ItemList.BeginRefresh();
        })));

        ItemList.ItemsSource = ViewModel.SecondaryMenuItems;
    }

    public async void Show()
    {
        isOpen = true;

        this.InputTransparent = false;
        this.Opacity = 0;
        this.IsVisible = true;
        Container.InputTransparent = false;
        Container.TranslationY = DeviceDisplay.MainDisplayInfo.Height / 4;
        ViewModel.ScreenWidthValue = MauiProgram.MainPage.ContentWidth;

        if (loadedImages.Contains(fullImgUrl))
        {
            ImageContainer_Img.Opacity = 1;
            ImageContainer_Img.Source = fullImgUrl;

            await Task.WhenAll(
                Container.TranslateTo(Container.X, 0, 300, Easing.SinOut),
                this.FadeTo(1, 300, Easing.SinIn));
            return;
        }

        ImageContainer_Img.Opacity = 0;

        Task<byte[]> imgTask = Task.Run(async () =>
        {
            byte[] bytes = new byte[0];
            try
            {
                HttpClient webClient = new();
                var response = await webClient.GetAsync(fullImgUrl);
                bytes = await response.Content.ReadAsByteArrayAsync();
                loadedImages.Add(fullImgUrl);
            }
            catch
            {

            }
            return bytes;
        });

        Trace.WriteLine("Starting image web request.");
        await Task.WhenAll(
            Container.TranslateTo(Container.X, 0, 300, Easing.SinOut),
            this.FadeTo(1, 300, Easing.SinIn),
            imgTask);

        Trace.WriteLine("Image downloaded, encoding.");

        byte[] bytes = imgTask.Result;
        if(bytes.Length > 0)
        {
            MemoryStream imageDecodeStream = new(imgTask.Result);
            this.ImageContainer_Img.Source = ImageSource.FromStream(() => imageDecodeStream);
        }
        else
        {
            this.ImageContainer_Img.Source = "emptyalbum.png";
        }
        await ImageContainer_Img.FadeTo(1, 750, Easing.SinIn);
    }

    public async Task<bool> Close()
    {
        if (isSecondaryPageOpen)
        {
            return false;
        }
        // TODO: Change to animation
        this.InputTransparent = true;
        isOpen = false;
        Container.InputTransparent = true;
        await Task.WhenAny(
            Container.TranslateTo(Container.X, DeviceDisplay.MainDisplayInfo.Height / 2, 300, Easing.SinIn),
            this.FadeTo(0, 300, Easing.SinIn)
            );
        return true;
    }

    public async Task<bool> ShowSecondarySelection()
    {

        return true;
    }


    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {

    }

    private void ItemList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        ListView? list = sender as ListView;
        if (list == null)
        {
            return;
        }

        ContextMenuItem? selectedItem = list.SelectedItem as ContextMenuItem;
        if(selectedItem == null)
        {
            return;
        }

        ItemList.SelectedItem = null;
        //SecondItemList.SelectedItem = null;
        if (selectedItem.action != null)
        {
            selectedItem.action.Invoke();
        }
    }
}