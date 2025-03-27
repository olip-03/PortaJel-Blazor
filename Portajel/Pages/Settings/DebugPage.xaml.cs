using Portajel.Structures;
using Portajel.Structures.ViewModels.Settings;

namespace Portajel.Pages.Settings;

public partial class DebugPage : ContentPage
{
    private SettingsPageViewModel _viewModel = new();
    public DebugPage()
	{
		InitializeComponent();
        _viewModel.ListItems = new()
        {
            new()
            {
                Title = "Debug Radio",
                Description = "Backend radio options for testing features.",
                Icon = "radio.png",
                NavigationLocation = "/settings/debug/radio"
            },
            new()
            {
                Title = "Debug Map",
                Description = "Test and configure map features.",
                Icon = "map.png",
                NavigationLocation = "/settings/debug/map"
            },
            new()
            {
                Title = "Debug Database",
                Description = "Test and configure the database.",
                Icon = "database.png",
                NavigationLocation = "/settings/debug/database"
            }
        };
        BindingContext = _viewModel;
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!e.CurrentSelection.Any()) return;
        if (e.CurrentSelection.First() is not ListItem listItem) return;
        if (sender is not CollectionView collectionView) return;
        collectionView.SelectedItem = null;
        await Shell.Current.GoToAsync(listItem.NavigationLocation);
    }
}