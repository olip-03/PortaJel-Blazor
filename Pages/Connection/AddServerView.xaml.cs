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
    
    public void SetConnectionStatus(MediaConnectionListing connectionListing, bool enabled)
    {
        foreach (var t in ViewModel.ConnectionListing)
        {
            if (t.Connection != connectionListing.Connection) continue;
            t.IsEnabled = enabled;
        }
    }

    public void CheckConnections()
    {
        if (_serverAdded)
        {
            ViewModel.CanContinue = true;
            return;
        }
        ViewModel.CanContinue = ViewModel.ConnectionListing.Any(t => t.IsEnabled);
    }
    
    private bool IsServerAdded(MediaConnectionListing connectionListing)
    {
        return MauiProgram.Server.GetServers().Any(item => item.GetAddress() == connectionListing.GetConnector().GetAddress());
    }
    [Obsolete("Obsolete")]
    private async void TryConnect()
    {
        if (Application.Current == null) return;
        if (_serverAdded)
        {
            await MauiProgram.SaveData();
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
        ViewModel.ConnectionListing.Add(new MediaConnectionListing(MediaServerConnection.Spotify));
        ViewModel.ConnectionListing.Add(new MediaConnectionListing(MediaServerConnection.Discogs));
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
        MediaConnectionListing connectionListing = (MediaConnectionListing)collection.SelectedItem;
        if (IsServerAdded(connectionListing))
        {
            collection.IsEnabled = true;
            return;
        }
        await Navigation.PushModalAsync(new AddConnectorView(this, connectionListing));
        collection.SelectedItem = null;
    }
    private async void ConnectionCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        if (checkBox.BindingContext == null) return;
        var connectionListing = (MediaConnectionListing)checkBox.BindingContext;
        if (IsServerAdded(connectionListing) && !e.Value)
        {
            CheckConnections();
            return;
        }
        if (IsServerAdded(connectionListing) || !e.Value)
        {
            CheckConnections();
            return;
        }
        await Navigation.PushModalAsync(new AddConnectorView(this, connectionListing));
        connectionListing.IsEnabled = checkBox.IsChecked;
        CheckConnections();
    }
}