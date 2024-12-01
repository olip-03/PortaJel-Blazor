using System.Collections.ObjectModel;
using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.ViewModels;

namespace PortaJel_Blazor.Classes;

public class AddConnectorPageView : BindableObject
{
    // Define an ObservableDictionary with string keys and ConnectorProperty values
    public ObservableCollection<Tuple<string, ConnectorProperty>> ConnectorPropertyDict=  new();
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}