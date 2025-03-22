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
        BindingContext = new MainViewModel();
        ScrollViewMain.Scrolled += ScrollViewMain_Scrolled;
    }

    private void ScrollViewMain_Scrolled(object? sender, ScrolledEventArgs e)
    {
        
        Trace.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
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
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}