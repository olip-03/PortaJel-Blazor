using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Diagnostics;

namespace PortaJel_Blazor.Shared;
public partial class ContextMenu : ContentView
{
    public bool isOpen { get; private set; } = false;

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
        Container.BindingContext = ViewModel;
    }

    public void UpdateData(BaseMusicItem? baseMusicItem = null,
        string? blurBase64 = null,
        int? opacity = 100,
        bool? isCircle = false)
    {
        if (baseMusicItem != null)
        {
            ViewModel.ContextMenuMainText = baseMusicItem.name ?? string.Empty;
            fullImgUrl = baseMusicItem.image.source ?? string.Empty;

            if (baseMusicItem is Album)
            {
                Album album = (Album)baseMusicItem;
                album.image.soureResolution = 500;

                ViewModel.ContextMenuItems = album.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                ViewModel.ContextMenuSubText = "Album • " + album.artistCongregate;
            }
            else if(baseMusicItem is Song)
            {
                Song song = (Song)baseMusicItem;
                song.image.soureResolution = 500;

                ViewModel.ContextMenuItems = song.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                ViewModel.ContextMenuSubText = "Song • " + song.artistCongregate;
            }
            else if (baseMusicItem is Artist)
            {
                Artist artist = (Artist)baseMusicItem;
                artist.image.soureResolution = 500;

                isCircle = true;
                ViewModel.ContextMenuItems = artist.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                ViewModel.ContextMenuSubText = "Artist";
            }
            else if (baseMusicItem is Playlist)
            {
                Playlist playlist = (Playlist)baseMusicItem;
                playlist.image.soureResolution = 500;

                ViewModel.ContextMenuItems = playlist.GetContextMenuItems().ToObservableCollection<ContextMenuItem>() ?? new();
                ViewModel.ContextMenuSubText = "Playlist";
            }

            if (baseMusicItem.contextMenuItems.Count <= 0 && ViewModel.ContextMenuItems != null)
            {
                ViewModel.ContextMenuItems.Add(new ContextMenuItem("Close", "light_close.png", new Task(() =>
                {
                    this.Close();
                })));
            }

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

    public async void Show()
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        isOpen = true;

        this.InputTransparent = false;
        this.Opacity = 0;
        Container.InputTransparent = false;
        Container.TranslationY = DeviceDisplay.MainDisplayInfo.Height / 4;

        Trace.WriteLine("Opening context menu.");

        if (loadedImages.Contains(fullImgUrl))
        {
            ImageContainer_Img.Opacity = 1;
            ImageContainer_Img.Source = fullImgUrl;

            await Task.WhenAll(
                Container.TranslateTo(Container.X, 0, 500, Easing.SinOut),
                this.FadeTo(1, 500, Easing.SinIn));
            return;
        }

        ImageContainer_Img.Opacity = 0;

        HttpClient webClient = new();
        Task<byte[]> imgTask = Task.Run(async () =>
        {
            byte[] bytes = new byte[0];
            try
            {
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
            Container.TranslateTo(Container.X, 0, 500, Easing.SinOut),
            this.FadeTo(1, 500, Easing.SinIn),
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

    public async void Close()
    {
        // TODO: Change to animation
        this.InputTransparent = true;
        isOpen = false;
        Container.InputTransparent = true;
        await Task.WhenAny(
            Container.TranslateTo(Container.X, DeviceDisplay.MainDisplayInfo.Height / 2, 750, Easing.SinIn),
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