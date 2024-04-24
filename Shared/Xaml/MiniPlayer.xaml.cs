using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Diagnostics;

namespace PortaJel_Blazor.Shared;

public partial class MiniPlayer : ContentView
{
    public MiniPlayerViewModel ViewModel { get; set; } = new();
    public double PositionX { get => TranslationX; private set { } }
    public double PositionY { get => PositionY; private set { } }
    public bool IsOpen { get; private set; } = false;
	public MiniPlayer()
	{
		InitializeComponent();
        this.BindingContext = ViewModel;
    }

    public async void Show()
    {
        this.Opacity = 0;
        this.TranslationY = 120;
        IsOpen = true;

        await Task.WhenAny(
                this.FadeTo(1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut));
    }

    public async void Hide()
    {
        IsOpen = false;

        await Task.WhenAny(
            this.FadeTo(0, 450, Easing.SinOut),
            this.TranslateTo(0, 120, 450, Easing.SinOut));
    }

    public void UpdateDate(Song[] songs)
    {
        ViewModel.Queue = songs.ToObservableCollection();

        // ViewModel.QueueBase64PlaceholderImg = blurHashBase64.ToList();
    }

    async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                this.TranslationY = this.TranslationY + e.TotalY;
                MauiProgram.MainPage.MediaPlayer.Opacity = 1;
                MauiProgram.MainPage.MediaPlayer.PositionY = MauiProgram.MainPage.MediaPlayer.PositionY + e.TotalY;
                break;

            case GestureStatus.Completed:                
                if(this.TranslationY < (MauiProgram.MainPage.ContentHeight / 3) * -1)
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                        MauiProgram.MainPage.MediaPlayer.TranslateTo(0, 0, 450, Easing.SinOut));
                    MauiProgram.MainPage.MediaPlayer.Open(animate: false);
                }
                else
                {
                    await Task.WhenAll(
                        this.TranslateTo(0, 0, 450, Easing.SinOut),
                        MauiProgram.MainPage.MediaPlayer.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
                    MauiProgram.MainPage.MediaPlayer.Close(animate: false);
                }
                break;
        }
    }

    private void MiniPlayer_Clicked(object sender, TappedEventArgs e)
    {

    }

    private void MiniPlayer_Swiped(object sender, SwipedEventArgs e)
    {

    }

    private void MiniPlayer_Btn_PlayToggle_Clicked(object sender, EventArgs e)
    {

    }

    private void MiniPlayer_Btn_FavToggle_Clicked(object sender, EventArgs e)
    {

    }
}