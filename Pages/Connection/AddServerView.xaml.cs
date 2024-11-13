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
    private readonly AddServerViewModel _vm = new();

    public UserCredentials UserCredentials { get; set; } = UserCredentials.Empty;
    public bool ServerPassed = false;
    public bool UserPassed = false;

    public AddServerView()
    {
        InitializeComponent();
        this.BindingContext = _vm;
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // LblErrormessage.Text = $"Username or password was incorrect.";
            });
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {

        });
    }

    private void SkipLogin()
    {
    }

    private void btn_skipLogin_Clicked(object sender, EventArgs e)
    {
        SkipLogin();
    }
    private void btn_connect_Clicked(object sender, EventArgs e)
    {
        TryConnect();
    }
    private async void ConnectionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not CollectionView collection) return;
        if (collection.SelectedItem == null) return;
        await Navigation.PushModalAsync(new AddConnectorView((MediaConnectionListing)collection.SelectedItem));
        collection.SelectedItem = null;
    }

    private async void ConnectionCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox collection) return;
        if (collection.BindingContext == null) return;
        var connectionListing = (MediaConnectionListing)collection.BindingContext;
        await Navigation.PushModalAsync(new AddConnectorView(connectionListing));
        connectionListing.IsEnabled = collection.IsChecked;
    }
}