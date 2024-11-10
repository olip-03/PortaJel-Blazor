using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Pages.Connection;

public partial class AddConnectorView : ContentPage
{
    public AddConnectorView()
    {
        InitializeComponent();
    }
    
    public AddConnectorView(MediaConnectionListing connectionListing)
    {
        InitializeComponent();
        var comps = connectionListing.GetConnector().Properties;
        foreach (var property in comps)
        {
            switch (property.Value.Value)
            {
                case string value:
                    PropertyStackLayout.Add(new Label() { Text = property.Value.Label, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) });
                    PropertyStackLayout.Add(new Entry() { Text = value, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) });
                    break;
                case bool boolValue:
                    PropertyStackLayout.Add(new Label() { Text = property.Value.Label, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) });
                    PropertyStackLayout.Add(new CheckBox() { IsChecked = false });
                    break;
                case List<string> list:
                    PropertyStackLayout.Add(new Label() { Text = property.Value.Label, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) });
                    PropertyStackLayout.Add(new Label() { Text = "List control here :(", FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) });
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
    
    private async void CancelButton_OnClicked(object sender, EventArgs e)
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}