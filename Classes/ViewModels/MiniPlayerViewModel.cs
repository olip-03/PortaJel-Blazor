using System.Collections.ObjectModel;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.ViewModels
{
    public class MiniPlayerViewModel : BindableObject
    {
        public List<string> QueueBase64PlaceholderImg = new();
        public static readonly BindableProperty QueueProperty = BindableProperty.Create(nameof(Queue), typeof(ObservableCollection<Song>), typeof(MiniPlayerViewModel), default(ObservableCollection<Song>));
        public ObservableCollection<Song>? Queue
        {
            get => (ObservableCollection<Song>?)GetValue(QueueProperty);
            set => SetValue(QueueProperty, value);
        }

        public static readonly BindableProperty PlaybackPercentageProperty = BindableProperty.Create(nameof(PlaybackPercentage), typeof(double), typeof(MiniPlayerViewModel), default(double));
        public double? PlaybackPercentage
        {
            get => (double?)GetValue(PlaybackPercentageProperty);
            set => SetValue(PlaybackPercentageProperty, value);
        }

        public static readonly BindableProperty PlayButtonSourceProperty = BindableProperty.Create(nameof(PlayButtonSource), typeof(string), typeof(MiniPlayerViewModel), default(string));
        public string? PlayButtonSource
        {
            get => (string?)GetValue(PlayButtonSourceProperty);
            set => SetValue(PlayButtonSourceProperty, value);
        }

        public static readonly BindableProperty FavButtonSourceProperty = BindableProperty.Create(nameof(FavButtonSource), typeof(string), typeof(MiniPlayerViewModel), default(string));
        public string? FavButtonSource
        {
            get => (string?)GetValue(FavButtonSourceProperty);
            set => SetValue(FavButtonSourceProperty, value);
        }

        public static readonly BindableProperty FavButtonColorProperty = BindableProperty.Create(nameof(FavButtonColor), typeof(Color), typeof(MiniPlayerViewModel), default(Color));
        public Color? FavButtonColor
        {
            get => (Color?)GetValue(FavButtonColorProperty);
            set => SetValue(FavButtonColorProperty, value);
        }

        // Background Image 
        public static readonly BindableProperty BackgroundImageSourceProperty = BindableProperty.Create(nameof(BackgroundImageSource), typeof(ImageSource), typeof(MediaControllerViewModel), default(ImageSource));
        public ImageSource BackgroundImageSource
        {
            get => (ImageSource)GetValue(BackgroundImageSourceProperty);
            set => SetValue(BackgroundImageSourceProperty, value);
        }
    }
}