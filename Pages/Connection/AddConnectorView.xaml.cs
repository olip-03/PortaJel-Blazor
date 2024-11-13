using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Pages.Connection;

public partial class AddConnectorView : ContentPage
{
    private IMediaServerConnector _serverConnector;
    private Dictionary<string, object> _entries = [];
    public AddConnectorView()
    {
        InitializeComponent();
    }

    public AddConnectorView(MediaConnectionListing connectionListing)
    {
        InitializeComponent();
        
        _serverConnector = connectionListing.GetConnector();
        var comps = _serverConnector.Properties;
        foreach (var property in comps)
        {
            switch (property.Value.Value)
            {
                // Create text entry
                case string value:
                    PropertyStackLayout.Add(new Label()
                    {
                        Text = property.Value.Label, 
                        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label))
                    });

                    Entry toAdd = new Entry()
                    {
                        Text = value,
                        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)),
                        IsPassword = property.Value.ProtectValue,
                        BindingContext = property.Value.Value
                    };
                    
                    _entries.Add(property.Key, toAdd);
                    PropertyStackLayout.Add(toAdd);
                    break;
                // Create list values
                case List<string> list:
                    PropertyStackLayout.Add(new Label()
                    {
                        Text = property.Value.Label, 
                        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label))
                    });
                    PropertyStackLayout.Add(new Label()
                    {
                        Text = "List control here :(", 
                        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label))
                    });
                    break;
            }
        }

        switch (connectionListing.Connection)
        {
            case MediaServerConnection.ServerConnector:
                Title.Text = "Server Connection";
                break;
            case MediaServerConnection.Database:
                Title.Text = "Database";
                break;
            case MediaServerConnection.Filesystem:
                Title.Text = "File System";
                break;
            case MediaServerConnection.Jellyfin:
                Title.Text = "Jellyfin";
                break;
            case MediaServerConnection.Spotify:
                Title.Text = "Spotify";
                break;
            case MediaServerConnection.Discogs:
                Title.Text = "Discogs";
                break;
        }
    }

    private async void ContinueButton_OnClicked(object sender, EventArgs e)
    {
        foreach (var entry in _entries)
        {
            var key = entry.Key;
            var value = _entries[key];
            switch (value)
            {
                case Entry inputEntry:
                    _serverConnector.Properties[key].Value = inputEntry.Text;
                    break;
            }
        }
        
        AuthenticationResponse auth = await _serverConnector.AuthenticateAsync();
        if (auth.IsSuccess)
        {
            // No wukkas
        }
        else
        {
            // Throw error
        }
    }

    private async void CancelButton_OnClicked(object sender, EventArgs e)
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}