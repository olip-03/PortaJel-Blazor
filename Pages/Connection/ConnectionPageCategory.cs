using System.Collections.ObjectModel;
using PortaJel_Blazor.Classes;

namespace PortaJel_Blazor.Pages.Connection;

public class ConnectionPageCategory : BindableObject
{
    public string CategoryTitle { get; set; } = string.Empty;
    public ObservableCollection<MediaConnectionListing> ConnectionListings { get; set; }= new();
}