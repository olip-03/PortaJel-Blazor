using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Diagnostics;

namespace PortaJel_Blazor.Shared;
public partial class ContextMenu : ContentView
{
    ContextMenuViewModel bindableProperties { get; set; } = new();
    private string fullImgUrl = string.Empty;

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
    }

    public void UpdateData(BaseMusicItem? baseMusicItem = null,
        string? blurBase64 = null,
        int? opacity = 100,
        bool? isCircle = false)
    {
        if (baseMusicItem != null)
        {
            bindableProperties.ContextMenuMainText = baseMusicItem.name ?? string.Empty;
            fullImgUrl = baseMusicItem.image.source ?? string.Empty;

            if (baseMusicItem is Album)
            {
                Album album = (Album)baseMusicItem;
                album.image.soureResolution = 500;

                bindableProperties.ContextMenuItems = album.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                bindableProperties.ContextMenuSubText = album.artistCongregate;
            }
            else if(baseMusicItem is Song)
            {
                Song song = (Song)baseMusicItem;
                song.image.soureResolution = 500;

                bindableProperties.ContextMenuItems = song.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                bindableProperties.ContextMenuSubText = song.artistCongregate;
            }
            else if (baseMusicItem is Artist)
            {
                Artist artist = (Artist)baseMusicItem;
                artist.image.soureResolution = 500;

                isCircle = true;
                bindableProperties.ContextMenuItems = artist.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                bindableProperties.ContextMenuSubText = string.Empty;
            }
            else if (baseMusicItem is Playlist)
            {
                Playlist playlist = (Playlist)baseMusicItem;
                playlist.image.soureResolution = 500;

                bindableProperties.ContextMenuItems = playlist.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                bindableProperties.ContextMenuSubText = string.Empty;
            }

            if (baseMusicItem.contextMenuItems.Count <= 0 && bindableProperties.ContextMenuItems != null)
            {
                bindableProperties.ContextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
                {
                    this.Close();
                })));
            }

            Container.BindingContext = bindableProperties;
            Opacity = opacity ?? 100;
            IsVisible = true;

            Container.TranslationY = DeviceDisplay.MainDisplayInfo.Height;
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
                    CornerRadius = new CornerRadius(125, 125, 125, 125)
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

    public async void Show()
    {
        this.InputTransparent = false;
        Container.InputTransparent = false;
        ImageContainer_Img.Opacity = 0;
        this.Opacity = 1;

        Trace.WriteLine("Opening context menu.");
        Trace.WriteLine("Starting image web request.");

        HttpClient webClient = new();

        Task<byte[]> imgTask = Task.Run(async () =>
        {
            var response = await webClient.GetAsync(fullImgUrl);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes;
        });

        await Task.WhenAll(
            Container.TranslateTo(Container.X, 0, 750, Easing.SinOut),
            this.FadeTo(1, 400, Easing.SinIn),
            imgTask);

        Trace.WriteLine("Image downloaded, encoding.");

        MemoryStream imageDecodeStream = new(imgTask.Result);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            this.ImageContainer_Img.Source = ImageSource.FromStream(() => imageDecodeStream);
            await ImageContainer_Img.FadeTo(1, 750, Easing.SinIn);
        });
    }

    public async void Close()
    {
        // TODO: Change to animation
        this.InputTransparent = true;
        Container.InputTransparent = true;
        await Task.WhenAny(
            Container.TranslateTo(Container.X, DeviceDisplay.MainDisplayInfo.Height, 750, Easing.SinIn),
            this.FadeTo(0, 750, Easing.SinIn)
            );
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

        if (selectedItem.action != null)
        {
            selectedItem.action.RunSynchronously();
        }
    }
}