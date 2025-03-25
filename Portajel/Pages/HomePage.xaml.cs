using Portajel.Pages.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Portajel.Pages;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
        MainViewModel viewModel = new();
        if (OperatingSystem.IsWindows())
        {
            viewModel.PageMargin = 0;
        }
        BindingContext = viewModel;
    }

    private void ScrollViewMain_Scrolled(object? sender, ScrolledEventArgs e)
    {
        
        Trace.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
    }

    private async void NavigateSettings(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settings");
    }
}

// Models/ImageItem.cs
public class ImageItem
{
    public string Source_ { get; set; }
}

public class MainViewModel : INotifyPropertyChanged
{
    private ObservableCollection<ImageItem> _sample;

    public int PageMargin = 10;
    public ObservableCollection<ImageItem> Sample
    {
        get => _sample;
        set
        {
            if (_sample != value)
            {
                _sample = value;
                OnPropertyChanged();
            }
        }
    }

    public MainViewModel()
    {
        LoadDummyData();
    }

    private void LoadDummyData()
    {
        Sample = new ObservableCollection<ImageItem>
        {
            new ImageItem { Source_ = "dotnet_bot.png" }, // Default MAUI image
            new ImageItem { Source_ = "cat1.png" },
            new ImageItem { Source_ = "cat2.png" },
            new ImageItem { Source_ = "cat3.png" },
            new ImageItem { Source_ = "cat4.png" }
        };
        PageMargin = 10;
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}