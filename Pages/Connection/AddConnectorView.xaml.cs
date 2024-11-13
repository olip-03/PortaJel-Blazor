using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private IMediaServerConnector _connector;
    private readonly MediaConnectionListing _connectionListing = null;
    private readonly Dictionary<string, object> _entries = [];
    private readonly AddServerView _parent = null;
    public AddConnectorView()
    {
        InitializeComponent();
    }

    public AddConnectorView(AddServerView parent, MediaConnectionListing connectionListing)
    {
        _connectionListing = connectionListing;
        _parent = parent;
        InitializeComponent();
        
        var comps = connectionListing.GetConnector().Properties;
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

    private async Task Close()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            _parent.CheckConnections();
            await _parent.Navigation.PopModalAsync(true);
        }
    }
    
    private async void ContinueButton_OnClicked(object sender, EventArgs e)
    {
        var collection = _connectionListing.GetConnector();
        foreach (var entry in _entries)
        {
            var key = entry.Key;
            var value = _entries[key];
            switch (value)
            {
                case Entry inputEntry:
                    collection.Properties[key].Value = inputEntry.Text;
                    break;
            }
        }

        try
        {
            AuthenticationResponse auth = await collection.AuthenticateAsync();
            // Add to main connections
            if (!auth.IsSuccess) return;
            _connector = collection;
            await Close();
            _parent.AddConnection(_connectionListing);
            // await Task.Delay(100);
            // MauiProgram.Server.AddServer(_connector);
            // _parent.PassConnection(_connectionListing); 
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
    }
    
    private async void CancelButton_OnClicked(object sender, EventArgs e)
    {
        await Close();
    }
}