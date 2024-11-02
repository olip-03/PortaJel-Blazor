using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes;

public class FilterItem(ConnectorDtoTypes type, string name, string icon)
{
    public ConnectorDtoTypes Type { get; set; } = type;
    public string Name { get; set; } = name;
    public string Icon { get; set; } = icon;
}