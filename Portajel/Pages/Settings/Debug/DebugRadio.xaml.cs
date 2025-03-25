using Portajel.Connections.Data.Radio.Search;
using Portajel.Connections.Services.Radio.RadioGarden;
using Portajel.Structures.ViewModels.Settings.Debug;
using System.Collections.ObjectModel;

namespace Portajel.Pages.Settings.Debug;

// https://jonasrmichel.github.io/radio-garden-openapi/
public partial class DebugRadio : ContentPage
{
    private DebugRadioViewModel viewmodel = new();
    // TODO: Convert radio controller to a service
    private RadioGardenController radioGarden = new();

    public DebugRadio()
	{
		InitializeComponent();
        BindingContext = viewmodel;
    }

    private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        viewmodel.SearchResults.Clear();

        if (sender is not SearchBar searchBar) return;
        if (String.IsNullOrWhiteSpace(searchBar.Text)) return;
        var results = await radioGarden.SearchAsync(searchBar.Text);
        if (results is null) return;
        foreach (var item in results.Hits.Hits)
        {
            viewmodel.SearchResults.Add(item);
        }
    }
}