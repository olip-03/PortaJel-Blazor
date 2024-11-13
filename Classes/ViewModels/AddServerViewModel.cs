using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Connectors.Database;
using PortaJel_Blazor.Classes.Connectors.FS;
using PortaJel_Blazor.Classes.Connectors.Jellyfin;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Pages.Connection;

namespace PortaJel_Blazor.Classes.ViewModels
{
    public class AddServerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IMediaServerConnector> Connections { get; set; } =
        [
            new ServerConnector(),
            new DatabaseConnector(),
        ];

        public ObservableCollection<MediaConnectionListing> ConnectionListing { get; set; } =
        [
            new MediaConnectionListing(MediaServerConnection.Filesystem),
            new MediaConnectionListing(MediaServerConnection.Jellyfin),
        ];

        private bool _canContinue = false;
        public bool CanContinue
        {
            get => _canContinue;
            set
            {
                if (_canContinue == value) return;
                _canContinue = value;
                OnPropertyChanged(nameof(CanContinue));
            }
        }
    
        public bool DebugMode { get; set; } = true;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}