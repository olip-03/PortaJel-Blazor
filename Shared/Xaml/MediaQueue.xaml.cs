using CommunityToolkit.Maui.Core.Extensions;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Diagnostics;
using System.Linq;

namespace PortaJel_Blazor.Shared;

public partial class MediaQueue : ContentView
{
    public QueueViewModel ViewModel = new();
    public bool IsOpen { get; private set; } = false;

    public MediaQueue()
	{
		InitializeComponent();
        BindingContext = ViewModel;
    }

    public async void Open(bool? animate = true)
    {
        Opacity = 1;
        TranslationY = MauiProgram.MainPage.ContentHeight;
        IsVisible = true;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, MauiProgram.MainPage.ContentHeight * -1, 450, Easing.SinOut),
                this.TranslateTo(0, 0, 450, Easing.SinOut));
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = MauiProgram.MainPage.ContentHeight * -1;
            TranslationY = 0;
        }
        IsOpen = true;
        BindingContext = null;
        BindingContext = ViewModel; // Refresh
    }

    public async void Close(bool? animate = true)
    {
        IsOpen = false;

        if (animate == true)
        {
            await Task.WhenAll(
                MauiProgram.MainPage.MainMiniPlayer.TranslateTo(0, 0, 450, Easing.SinOut),
                this.TranslateTo(0, MauiProgram.MainPage.ContentHeight, 450, Easing.SinOut));
        }
        else
        {
            MauiProgram.MainPage.MainMiniPlayer.TranslationY = 0;
            TranslationY = MauiProgram.MainPage.ContentHeight;
        }

        IsVisible = false;
    }

    public void UpdateData(SongGroupCollection? songGroups = null, int? playFromIndex = 0)
    {
        if (songGroups != null)
        {
            SongGroup? sg = songGroups.FirstOrDefault(sg => sg.Name == "Previous");
            if(sg != null)
            {
                songGroups.Remove(sg);
            }

            ViewModel.SongQueue = songGroups.SongGroups;
        }

        // Update time tracking
        PlaybackInfo? timeInfo = MauiProgram.MediaService.GetPlaybackTimeInfo();
        MauiProgram.MainPage.MainMiniPlayer.UpdateTimestamp(timeInfo);
    }

    public void InsertIntoQueue(Song song)
    {
        if (MauiProgram.MediaService == null) return;
        if(ViewModel.SongQueue  == null) ViewModel.SongQueue = MauiProgram.MediaService.GetQueue();

        try
        {
            SongGroupCollection sgc = MauiProgram.MediaService.GetQueue();
            SongGroup? queueGroup = ViewModel.SongQueue.FirstOrDefault(sg => sg.Name == "Queue");

            if (queueGroup != null)
            {
                int insertInto = sgc.QueueCount;
                ViewModel.SongQueue.FirstOrDefault(sg => sg.Name == "Queue").Insert(insertInto, song);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
        }
    }

    private void Btn_Close_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private void Btn_ContextMenu_Clicked(object sender, EventArgs e)
    {

    }

    private void DurationSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {

    }

    private void DurationSlider_DragStarted(object sender, EventArgs e)
    {

    }

    private void DurationSlider_DragCompleted(object sender, EventArgs e)
    {

    }

    private void Btn_PlayToggle_Pressed(object sender, EventArgs e)
    {

    }

    private void Btn_PlayToggle_Released(object sender, EventArgs e)
    {

    }
}