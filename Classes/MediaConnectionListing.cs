using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Connectors.Database;
using PortaJel_Blazor.Classes.Connectors.FS;
using PortaJel_Blazor.Classes.Connectors.Jellyfin;
using PortaJel_Blazor.Classes.Connectors.Spotify;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes
{
    public sealed class MediaConnectionListing : INotifyPropertyChanged
    {
        private readonly IMediaServerConnector _connector = null;
        private bool _isEnabled = false;
        public string ImageUrl { get; private set; }
        public string PrimaryText { get; set; }
        public string SecondaryText { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                    OnPropertyChanged(nameof(ButtonOpacity)); // Also update ButtonOpacity
                }
            }
        }
        private double _buttonOpacity = 1;
        public double ButtonOpacity
        {
            get
            {
                if (!IsEnabled) return 0.4;
                return 1;
            }
            set => _buttonOpacity = value;
        }
        public MediaServerConnection Connection { get; set; }
        public MediaConnectionListing(MediaServerConnection connection)
        {
            Connection = connection;
            switch (Connection)
            {
                case MediaServerConnection.ServerConnector:
                    ImageUrl = "Logo_Folder.png";
                    PrimaryText = "Database";
                    SecondaryText = "Add a connection to multiple servers.";
                    break;
                case MediaServerConnection.Database:
                    ImageUrl = "Logo_Folder.png";
                    PrimaryText = "Database";
                    SecondaryText = "Add a connection to a local database.";
                    break;
                case MediaServerConnection.Filesystem:
                    ImageUrl = "Logo_Folder.png";
                    PrimaryText = "File System";
                    SecondaryText = "Add a connection to a music directory.";
                    _connector = new FileSystemConnector();
                    break;
                case MediaServerConnection.Jellyfin:
                    ImageUrl = "Logo_Jellyfin.png";
                    PrimaryText = "Jellyfin Media Server";
                    SecondaryText = "Add a connection to a JellyFin Server.";
                    _connector = new JellyfinServerConnector();
                    break;
                case MediaServerConnection.Spotify:
                    ImageUrl = "Logo_Spotify.png";
                    PrimaryText = "Spotify Account";
                    SecondaryText = "Add a connection to your Spotify account.";
                    _connector = new SpotifyServerConnector();
                    break;
                case MediaServerConnection.Discogs:
                    ImageUrl = "Logo_Discogs.png";
                    PrimaryText = "Discogs Account";
                    SecondaryText = "Add a connection to your Discogs account.";
                    _connector =  new SpotifyServerConnector();
                    break;
            }
        }
        public IMediaServerConnector GetConnector()
        {
            return _connector;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}