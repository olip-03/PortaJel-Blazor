using System.Collections.ObjectModel;
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

    public UserCredentials UserCredentials { get; set; } = UserCredentials.Empty;
    public bool ServerPassed = false;
    public bool UserPassed = false;

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
    
    private async void TryConnect()
    {
        try
        {
            AuthenticationResponse response = await ServerConnecter.AuthenticateAsync();
            UserPassed = response.IsSuccess;
            ServerPassed = true;
        }
        catch (HttpRequestException)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                //LblErrormessage.Text = $"Unable to connect to {UserCredentials.ServerAddress}.";
            });
            ServerPassed = false;
        }
        catch (Exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // LblErrormessage.Text = $"Generic fault! Please try again.";
            });
        }

        if (ServerPassed && !UserPassed)
        {
            ViewModel.ConnectionListing.Clear();
            ViewModel.ConnectionListing.Add(new MediaConnectionListing(MediaServerConnection.Spotify));
            ViewModel.ConnectionListing.Add(new MediaConnectionListing(MediaServerConnection.Discogs));
        }
    }
    private void SkipLogin()
    {
        
    }
    private void btn_connect_Clicked(object sender, EventArgs e)
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