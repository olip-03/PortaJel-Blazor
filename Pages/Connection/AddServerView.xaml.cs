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
        
        MediaConnectionListing[] connectionListing = new MediaConnectionListing[]
        {
            new MediaConnectionListing(MediaServerConnection.Filesystem),
            new MediaConnectionListing(MediaServerConnection.Jellyfin)
        };
        
        MediaConnectionListing[] serviceListing = new MediaConnectionListing[]
        {
            new MediaConnectionListing(MediaServerConnection.Spotify),
            new MediaConnectionListing(MediaServerConnection.Discogs)
        };
        
        this.BindingContext = _vm;
    }

    private async void TryConnect()
    {
        //      if (EntryServer.Text.StartsWith("http") && !EntryServer.Text.Contains("https"))
        //      {
        //          if (App.Current != null && App.Current.MainPage != null)
        //          { // fuck i am so sick of these null reference checks
        //              bool answer = await App.Current.MainPage.DisplayAlert("Warning?", $"Insecure servers are currently not fully supported, are you sure you want to use this connection?", "Yes", "No");
        //              if (answer == false)
        //              {
        //                  return;
        //              }
        //          }
        //          System.Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--unsafely-treat-insecure-origin-as-secure={EntryServer.Text}");
        //      }

        //      UserCredentials.ServerAddress = EntryServer.Text;
        //UserCredentials.UserName = EntryUsername.Text;
        //      UserCredentials.Password = EntryPassword.Text;

        //      EntryServer.IsReadOnly = true;
        //      EntryUsername.IsReadOnly = true;
        //      EntryPassword.IsReadOnly = true;

        //      LblErrormessage.Opacity = 0;

        //      EntryServer.Opacity = 0.5;
        //      EntryUsername.Opacity = 0.5;
        //      EntryPassword.Opacity = 0.5;

        // serverConnecter = new(entry_server.Text);

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
            //EntryServer.IsReadOnly = false;
            //EntryUsername.IsReadOnly = false;
            //EntryPassword.IsReadOnly = false;

            //await Task.WhenAll(
            //    EntryServer.FadeTo(1, 250, Easing.SinOut),
            //    EntryUsername.FadeTo(1, 250, Easing.SinOut),
            //    EntryPassword.FadeTo(1, 250, Easing.SinOut));

            //if(ServerPassed && UserPassed)
            //{
            //    LblErrormessage.Opacity = 0;
            //    // Close this page  
            //    if (Navigation.ModalStack.Count > 0)
            //    {
            //        MauiProgram.Server.AddServer(serverConnecter);
            //        await Navigation.PopModalAsync();
            //    }
            //}
            //else
            //{
            //    await LblErrormessage.FadeTo(1, 250, Easing.SinOut);
            //}
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