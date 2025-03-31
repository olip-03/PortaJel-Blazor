using Portajel.Connections;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Connections.Services.Database;
using Portajel.Structures.Functional;
using Portajel.Structures.ViewModels.Settings.Connections;
using System;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Portajel.Pages.Settings.Connections;
public partial class ViewConnectionPage : ContentPage, IQueryAttributable
{
    private string url = default!;

    private Dictionary<string, ConnectorProperty> _connectionProperties = default!;
    private ViewConnectionViewModel _viewModel = new();

    private IServerConnector _server = default!;
    private IDbConnector _database = default!;

    public ViewConnectionPage(IServerConnector serverConnector, IDbConnector dbConnector)
    {
        _server =  serverConnector;
        _database = dbConnector;
        InitializeComponent();
        BindingContext = _viewModel;
    }

    // This method will receive all navigation parameters at once
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _connectionProperties = (Dictionary<string, ConnectorProperty>)query["Properties"];
        url = (string)_connectionProperties["URL"].Value;
        foreach (var item in _connectionProperties)
        {
            if (!item.Value.UserVisisble) continue;
            _viewModel.ConnectionItems.Add((ConnectorProperty)item.Value);
        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) return;
        if (_connectionProperties is null) return;
        var connectorProperty = _connectionProperties.FirstOrDefault(c => c.Value == entry.BindingContext);
        if (connectorProperty.Value != null)
        {
            connectorProperty.Value.Value = e.NewTextValue;
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var srv = _server.Servers.First(s => s.GetAddress() == url);
        _server.Servers.Remove(srv);
        await SaveHelper.SaveData(_server);
        await Shell.Current.GoToAsync("..");
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        var toast = Toast.Make("Saving...", ToastDuration.Short, 14);
        var server = _server.Servers.First(s => s.GetAddress() == url);

        AuthResponse checkSrv = await server.AuthenticateAsync();

        if (checkSrv.IsSuccess)
        {
            await SaveHelper.SaveData(_server);
            toast = Toast.Make("Saved data successfully", ToastDuration.Short, 14);
            await toast.Show();
        }
        else
        {
            toast = Toast.Make("Failed to save: Could not authenticate with the server.", ToastDuration.Long, 14);
            await toast.Show();
        }
    }
}
