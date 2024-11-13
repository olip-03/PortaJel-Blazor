using System.Collections.ObjectModel;
using System.Diagnostics;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Enum;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.ViewModels;

namespace PortaJel_Blazor.Pages.Connection;

public partial class AddServerView : ContentPage
{
    public IMediaServerConnector ServerConnecter { get; private set; } = null;
    private AddServerViewModel ViewModel { get; set; } = new();

    private bool _serverAdded = false;

    public AddServerView()
    {
        InitializeComponent();
        BindingContext = ViewModel;
    }

    public void AddConnection(MediaConnectionListing connectionListing)
    {
        if(MauiProgram.Server.GetServers().Contains(connectionListing.GetConnector())) return;
        foreach (var t in ViewModel.ConnectionListing)
        {
            if (t.Connection != connectionListing.Connection) continue;
            t.IsEnabled = true;
            MauiProgram.Server.AddServer(connectionListing.GetConnector());
        }
    }

    public void CheckConnections()
    {
        foreach (var t in ViewModel.ConnectionListing)
        {
            if (!t.IsEnabled) continue;
            ViewModel.CanContinue = true;
            break;
        }
    }
    
    [Obsolete("Obsolete")]
    private async void TryConnect()
    {
        if (Application.Current == null) return;
        if (_serverAdded)
        {
            Application.Current.MainPage = new MainPage();
            return;
        }
            
        try
        {
            AuthenticationResponse response = await MauiProgram.Server.AuthenticateAsync();
            _serverAdded = response.IsSuccess;
        }
        catch (HttpRequestException htex)
        {
            _serverAdded = false;
            Trace.WriteLine(htex.Message);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
        
        ViewModel.ConnectionListing.Clear();
        await Task.Delay(500);
        ViewModel.ConnectionListing =
        [
            new MediaConnectionListing(MediaServerConnection.Spotify),
            new MediaConnectionListing(MediaServerConnection.Discogs),
        ];
    }
    [Obsolete("Obsolete")]
    private void ContinueButton_OnClicked(object sender, EventArgs e)
    {
        TryConnect();
    }
    private async void ConnectionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not CollectionView collection) return;
        if (collection.SelectedItem == null) return;
        // Check if item is already in collection
        MediaConnectionListing connectionListing = (MediaConnectionListing)collection.SelectedItem;
        foreach (var item in MauiProgram.Server.GetServers())
        {
            if (item.GetType() == connectionListing.Connection)
            {
                collection.IsEnabled = false;
            }
        }
        await Navigation.PushModalAsync(new AddConnectorView(this, connectionListing));
        collection.SelectedItem = null;
    }

    private async void ConnectionCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox collection) return;
        if (collection.BindingContext == null) return;
        var connectionListing = (MediaConnectionListing)collection.BindingContext;
        await Navigation.PushModalAsync(new AddConnectorView(this, connectionListing));
        connectionListing.IsEnabled = collection.IsChecked;
    }
}