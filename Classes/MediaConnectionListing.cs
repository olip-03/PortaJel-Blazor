using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MediaConnectionListing
    {
        private IMediaServerConnector _mediaServerConnector;
        public string ImageUrl { get; private set; }
        public string PrimaryText { get; private set; }
        public string SecondaryText { get; private set; }
        public bool IsEnabled { get; set; } = false;
        public double ButtonOpacity
        {
            get
            {
                if (!IsEnabled) return 0.4;
                return 1;
            }
        }

        public MediaServerConnection Connection { get; set; }

        public MediaConnectionListing(MediaServerConnection connection)
        {
            Connection = connection;

            switch (connection)
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
                    _mediaServerConnector = new FileSystemConnector();
                    break;
                case MediaServerConnection.Jellyfin:
                    ImageUrl = "Logo_Jellyfin.png";
                    PrimaryText = "Jellyfin Media Server";
                    SecondaryText = "Add a connection to a JellyFin Server.";
                    _mediaServerConnector = new JellyfinServerConnector();
                    break;
                case MediaServerConnection.Spotify:
                    ImageUrl = "Logo_Spotify.png";
                    PrimaryText = "Spotify Account";
                    SecondaryText = "Add a connection to your Spotify account.";
                    _mediaServerConnector = new SpotifyServerConnector();
                    break;
                case MediaServerConnection.Discogs:
                    ImageUrl = "Logo_Discogs.png";
                    PrimaryText = "Discogs Account";
                    SecondaryText = "Add a connection to your Discogs account.";
                    _mediaServerConnector = new SpotifyServerConnector();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connection), connection, null);
            }
        }

        public IMediaServerConnector GetConnector()
        {
            return _mediaServerConnector;
        }

        public void SetConnector(IMediaServerConnector mediaServerConnector)
        {
            _mediaServerConnector = mediaServerConnector;
        }
    }
}