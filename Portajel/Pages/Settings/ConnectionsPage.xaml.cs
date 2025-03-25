using Portajel.Connections.Interfaces;
using Portajel.Connections;
using Portajel.Structures;
using Portajel.Structures.ViewModels.Settings;
using Portajel.Components.Modal;
using Portajel.Connections.Services.Jellyfin;
using System.Data.Common;
using Portajel.Connections.Services.Database;
using Portajel.Structures.Functional;

namespace Portajel.Pages.Settings;

public partial class ConnectionsPage : ContentPage
{
    private readonly DatabaseConnector _dbConnector;
    private readonly ServerConnector _serverConnector;

    private ConnectionsPageViewModel _viewModel = new();

    public ConnectionsPage(IMediaServerConnector serverConnector, IDbConnector dbConnector)
	{
		InitializeComponent();
        _dbConnector = (DatabaseConnector)dbConnector;
        _serverConnector = (ServerConnector)serverConnector;
        RefreshUI();
        BindingContext = _viewModel;
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!e.CurrentSelection.Any()) return;
        if(e.CurrentSelection.First() is not ListItem listItem) return;
        if (sender is not CollectionView collectionView) return;
        await Shell.Current.GoToAsync(listItem.NavigationLocation);
        collectionView.SelectedItem = null;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        JellyfinServerConnector newServer = new JellyfinServerConnector(_dbConnector);
        await Navigation.PushModalAsync(new ModalAddServer(_serverConnector, newServer) { OnLoginSuccess=OnLoginSuccessAsync }, true);
    }

    private void OnLoginSuccessAsync(IMediaServerConnector server)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        _viewModel.Connectors.Clear();
        foreach (var item in _serverConnector.GetServers())
        {
            _viewModel.Connectors.Add(item);
        }
    }
}