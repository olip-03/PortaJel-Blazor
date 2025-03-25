namespace Portajel.Pages.Settings.Debug;

public partial class DebugMap : ContentPage
{
    // https://mapsui.com/v5/custom-style-renders/
    public DebugMap()
	{
		InitializeComponent();
        var mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        Content = mapControl;
    }
}