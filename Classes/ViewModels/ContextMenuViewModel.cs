using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Collections.ObjectModel;

namespace PortaJel_Blazor.Classes
{
    public class ContextMenuViewModel : BindableObject
    {
        public static readonly BindableProperty ContextMenuItemsProperty = BindableProperty.Create(nameof(ContextMenuItems), typeof(ObservableCollection<ContextMenuItem>), typeof(ContextMenuViewModel), default(ObservableCollection<ContextMenuItem>));
        public ObservableCollection<ContextMenuItem>? ContextMenuItems
        {
            get => (ObservableCollection<ContextMenuItem>?)GetValue(ContextMenuItemsProperty);
            set => SetValue(ContextMenuItemsProperty, value);
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
    }
}
