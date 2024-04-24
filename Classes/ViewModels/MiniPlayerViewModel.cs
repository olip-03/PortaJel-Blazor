using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Collections.ObjectModel;

namespace PortaJel_Blazor.Classes
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
    }
}