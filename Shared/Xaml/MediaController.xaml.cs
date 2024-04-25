using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.ViewModels;
using PortaJel_Blazor.Data;
using System.Diagnostics;
namespace PortaJel_Blazor.Shared;

public partial class MediaController : ContentView
{
    public MediaControllerViewModel ViewModel { get; set; } = new();

    public bool IsOpen { get; private set; } = false;
    public bool IsQueueOpen { get; private set; } = false;

    public double PositionX { get => TranslationX; set => TranslationX = value; }
    public double PositionY { get => TranslationY; set => TranslationY = value; }

    public MediaController()
	{
		InitializeComponent();
        this.BindingContext = ViewModel;
	}

    public async void Open(bool? animate = true)
    {
        Opacity = 1;
        TranslationY = MauiProgram.MainPage.ContentHeight;
        IsOpen = true;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MiniPlayerController.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut));

                
        }
        else
        {
            MauiProgram.MainPage.MiniPlayerController.TranslationY = MauiProgram.MainPage.ContentHeight * -1;
            TranslationY = 0; 
        }
    }

    public async void Close(bool? animate = true)
    {
        IsOpen = false;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MiniPlayerController.TranslateTo(0, 0, 450, Easing.SinOut),
                this.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
        }
        else
        {
            MauiProgram.MainPage.MiniPlayerController.TranslationY = 0;
            TranslationY = MauiProgram.MainPage.ContentHeight;
        }

    }

    public void UpdateData(Song[] songs, int? playFromIndex = 0, bool? animate = false)
    {
        ViewModel.Queue = songs.ToObservableCollection();
    }

    private void Player_Btn_Close_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private void Player_Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Repeat_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Previous_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Play_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Next_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Shuffle_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_Fav_Clicked(object sender, EventArgs e)
    {

    }

    private void Player_Btn_ShowQueue_Clicked(object sender, EventArgs e)
    {

    }

    private void ImgCarousel_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {

    }

    private void ImgCarousel_ScrollToRequested(object sender, ScrollToRequestEventArgs e)
    {

    }

    private void ImgCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {

    }

    private void Player_DurationSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {

    }

    private void Player_DurationSlider_DragStarted(object sender, EventArgs e)
    {

    }

    private void Player_DurationSlider_DragCompleted(object sender, EventArgs e)
    {

    }

    private void Queue_Btn_Close_Clicked(object sender, EventArgs e)
    {

    }

    private void Queue_Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {

    }
}