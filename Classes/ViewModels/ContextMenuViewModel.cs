using System.Collections.ObjectModel;

namespace PortaJel_Blazor.Classes.ViewModels
{
    public class ContextMenuViewModel : BindableObject
    {
        public static readonly BindableProperty ContextMenuItemsProperty = BindableProperty.Create(nameof(ContextMenuItems), typeof(ObservableCollection<ContextMenuItem>), typeof(ContextMenuViewModel), default(ObservableCollection<ContextMenuItem>));
        public ObservableCollection<ContextMenuItem>? ContextMenuItems
        {
            get => (ObservableCollection<ContextMenuItem>?)GetValue(ContextMenuItemsProperty);
            set => SetValue(ContextMenuItemsProperty, value);
        }

        public static readonly BindableProperty SecondaryMenuItemsProperty = BindableProperty.Create(nameof(SecondaryMenuItems), typeof(ObservableCollection<ContextMenuItem>), typeof(ContextMenuViewModel), default(ObservableCollection<ContextMenuItem>));
        public ObservableCollection<ContextMenuItem>? SecondaryMenuItems
        {
            get => (ObservableCollection<ContextMenuItem>?)GetValue(SecondaryMenuItemsProperty);
            set => SetValue(SecondaryMenuItemsProperty, value);
        }

        public static readonly BindableProperty ContextMenuBackgroundImageProperty = BindableProperty.Create(nameof(ContextMenuBackgroundImage), typeof(string), typeof(ContextMenuViewModel), string.Empty);
        public string ContextMenuBackgroundImage
        {
            get => (string)GetValue(ContextMenuBackgroundImageProperty);
            set => SetValue(ContextMenuBackgroundImageProperty, value);
        }

        public static readonly BindableProperty ContextMenuImageProperty = BindableProperty.Create(nameof(ContextMenuImage), typeof(string), typeof(ContextMenuViewModel), string.Empty);
        public string ContextMenuImage
        {
            get => (string)GetValue(ContextMenuImageProperty);
            set => SetValue(ContextMenuImageProperty, value);
        }

        public static readonly BindableProperty ContextMenuMainTextProperty = BindableProperty.Create(nameof(ContextMenuMainText), typeof(string), typeof(ContextMenuViewModel), string.Empty);
        public string ContextMenuMainText
        {
            get => (string)GetValue(ContextMenuMainTextProperty);
            set => SetValue(ContextMenuMainTextProperty, value);
        }

        public static readonly BindableProperty ContextMenuSubTextProperty = BindableProperty.Create(nameof(ContextMenuSubText), typeof(string), typeof(ContextMenuViewModel), string.Empty);
        public string ContextMenuSubText
        {
            get => (string)GetValue(ContextMenuSubTextProperty);
            set => SetValue(ContextMenuSubTextProperty, value);
        }

        public static readonly BindableProperty HeaderHeightValueProperty = BindableProperty.Create(nameof(HeaderHeightValue), typeof(double), typeof(MediaControllerViewModel), default(double));
        public double HeaderHeightValue
        {
            get => (double)GetValue(HeaderHeightValueProperty);
            set => SetValue(HeaderHeightValueProperty, value);
        }

        public static readonly BindableProperty ScreenWidthValueProperty = BindableProperty.Create(nameof(ScreenWidthValue), typeof(double), typeof(MediaControllerViewModel), default(double));
        public double ScreenWidthValue
        {
            get => (double)GetValue(ScreenWidthValueProperty);
            set => SetValue(ScreenWidthValueProperty, value);
        }
    }
}
