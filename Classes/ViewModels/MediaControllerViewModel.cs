using PortaJel_Blazor.Data;
using System.Collections.ObjectModel;

namespace PortaJel_Blazor.Classes
{
    public class MediaControllerViewModel : BindableObject
    {
        public List<string> QueueBase64PlaceholderImg = new();
        public static readonly BindableProperty QueueProperty = BindableProperty.Create(nameof(Queue), typeof(ObservableCollection<Song>), typeof(MiniPlayerViewModel), default(ObservableCollection<Song>));
        public ObservableCollection<Song>? Queue
        {
            get => (ObservableCollection<Song>?)GetValue(QueueProperty);
            set => SetValue(QueueProperty, value);
        }

        // Represents time passed as a Double, in full MS
        public static readonly BindableProperty PlaybackPercentageProperty = BindableProperty.Create(nameof(PlaybackValue), typeof(double), typeof(MediaControllerViewModel), default(double));
        public double PlaybackValue
        {
            get => (double)GetValue(PlaybackPercentageProperty);
            set => SetValue(PlaybackPercentageProperty, value);
        }

        // Represents time passed as a string in MM:SS format
        public static readonly BindableProperty PlaybackTimeValueProperty = BindableProperty.Create(nameof(PlaybackTimeValue), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string PlaybackTimeValue
        {
            get => (string)GetValue(PlaybackTimeValueProperty);
            set => SetValue(PlaybackTimeValueProperty, value);
        }

        // Represents song length as a Double, in full MS
        public static readonly BindableProperty PlaybackMaximumProperty = BindableProperty.Create(nameof(PlaybackMaximum), typeof(double), typeof(MediaControllerViewModel), default(double));
        public double PlaybackMaximum
        {
            get => (double)GetValue(PlaybackMaximumProperty);
            set => SetValue(PlaybackMaximumProperty, value);
        }

        // Represents time passed as a string in MM:SS format
        public static readonly BindableProperty PlaybackMaximumTimeValueProperty = BindableProperty.Create(nameof(PlaybackMaximumTimeValue), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string PlaybackMaximumTimeValue
        {
            get => (string)GetValue(PlaybackMaximumTimeValueProperty);
            set => SetValue(PlaybackMaximumTimeValueProperty, value);
        }

        // Represents the play button source image
        public static readonly BindableProperty PlayButtonSourceProperty = BindableProperty.Create(nameof(PlayButtonSource), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string? PlayButtonSource
        {
            get => (string?)GetValue(PlayButtonSourceProperty);
            set => SetValue(PlayButtonSourceProperty, value);
        }

        // Represents the favourite button source image
        public static readonly BindableProperty FavButtonSourceProperty = BindableProperty.Create(nameof(FavButtonSource), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string? FavButtonSource
        {
            get => (string?)GetValue(FavButtonSourceProperty);
            set => SetValue(FavButtonSourceProperty, value);
        }

        // Represents the favourite button source image
        public static readonly BindableProperty RepeatButtonSourceProperty = BindableProperty.Create(nameof(RepeatButtonSource), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string? RepeatButtonSource
        {
            get => (string?)GetValue(RepeatButtonSourceProperty);
            set => SetValue(RepeatButtonSourceProperty, value);
        }

        // Represents the fav button image color
        public static readonly BindableProperty FavButtonColorProperty = BindableProperty.Create(nameof(FavButtonColor), typeof(Color), typeof(MediaControllerViewModel), default(Color));
        public Color? FavButtonColor
        {
            get => (Color?)GetValue(FavButtonColorProperty);
            set => SetValue(FavButtonColorProperty, value);
        }

        public static readonly BindableProperty PlayingFromCollectionTitleProperty = BindableProperty.Create(nameof(PlayingFromCollectionTitle), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string? PlayingFromCollectionTitle
        {
            get => (string?)GetValue(PlayingFromCollectionTitleProperty);
            set => SetValue(PlayingFromCollectionTitleProperty, value);
        }

        public static readonly BindableProperty PlayingFromTitleProperty = BindableProperty.Create(nameof(PlayingFromTitle), typeof(string), typeof(MediaControllerViewModel), default(string));
        public string? PlayingFromTitle
        {
            get => (string?)GetValue(PlayingFromTitleProperty);
            set => SetValue(PlayingFromTitleProperty, value);
        }

        public static readonly BindableProperty BackgroundImageSourceProperty = BindableProperty.Create(nameof(BackgroundImageSource), typeof(ImageSource), typeof(MediaControllerViewModel), default(ImageSource));
        public ImageSource BackgroundImageSource
        {
            get => (ImageSource)GetValue(BackgroundImageSourceProperty);
            set => SetValue(BackgroundImageSourceProperty, value);
        }

        // Represents time passed as a Double, in full MS
        public static readonly BindableProperty HeaderHeightValueProperty = BindableProperty.Create(nameof(HeaderHeightValue), typeof(double), typeof(MediaControllerViewModel), default(double));
        public double HeaderHeightValue
        {
            get => (double)GetValue(HeaderHeightValueProperty);
            set => SetValue(HeaderHeightValueProperty, value);
        }
    }
}
