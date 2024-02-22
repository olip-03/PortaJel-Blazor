using PortaJel_Blazor.Data;
using Jellyfin.Sdk;
using Microsoft.Maui.Controls;
using PortaJel_Blazor.Classes;

namespace PortaJel_Blazor.Pages.Xaml;

public partial class AddServerView : ContentPage
{
    public ServerConnecter serverConnecter { get; private set; } = new();

	public UserCredentials UserCredentials { get; set; } = UserCredentials.Empty;
    public bool ServerPassed = false;
    public bool UserPassed = false;
    public AddServerView()
	{
		InitializeComponent();
    }
    public Task AwaitClose(Page page)
    {
        while (MauiProgram.mainPage.GetNavigation().ModalStack.Contains(page))
        {
            // sit around and wait a while
        }
        return Task.CompletedTask;
    }
	private async void TryConnect()
	{
        if (entry_server.Text.StartsWith("http") && !entry_server.Text.Contains("https"))
        {
            if (App.Current != null && App.Current.MainPage != null)
            { // fuck i am so sick of these null reference checks
                bool answer = await App.Current.MainPage.DisplayAlert("Warning?", $"Insecure servers are currently not fully supported, are you sure you want to use this connection?", "Yes", "No");
                if (answer == false)
                {
                    return;
                }
            }
            System.Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--unsafely-treat-insecure-origin-as-secure={entry_server.Text}");
        }

        UserCredentials.ServerAddress = entry_server.Text;
		UserCredentials.UserName = entry_username.Text;
        UserCredentials.Password = entry_password.Text;

        entry_server.IsReadOnly = true;
        entry_username.IsReadOnly = true;
        entry_password.IsReadOnly = true;

        lbl_errormessage.Opacity = 0;

        entry_server.Opacity = 0.5;
        entry_username.Opacity = 0.5;
        entry_password.Opacity = 0.5;

        try
        {
            ServerPassed = await serverConnecter.AuthenticateAddressAsync(entry_server.Text);
            UserPassed = await serverConnecter.AuthenticateUser(entry_username.Text, entry_password.Text);

            if (!ServerPassed)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    lbl_errormessage.Text = $"Unable to connect to {UserCredentials.ServerAddress}.";
                });
            }
            else if (!UserPassed)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    lbl_errormessage.Text = $"Username or password was incorrect.";
                });
            }
        }
        catch (Exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lbl_errormessage.Text = $"Generic fault! Please try again.";
            });
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            entry_server.IsReadOnly = false;
            entry_username.IsReadOnly = false;
            entry_password.IsReadOnly = false;

            await Task.WhenAll(
                entry_server.FadeTo(1, 250, Easing.SinOut),
                entry_username.FadeTo(1, 250, Easing.SinOut),
                entry_password.FadeTo(1, 250, Easing.SinOut));

            if(ServerPassed && UserPassed)
            {
                lbl_errormessage.Opacity = 0;
                // Close this page  
                if (Navigation.ModalStack.Count > 0)
                {
                    MauiProgram.AddServer(serverConnecter);
                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                await lbl_errormessage.FadeTo(1, 250, Easing.SinOut);
            }
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
}