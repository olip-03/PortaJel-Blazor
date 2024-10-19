namespace PortaJel_Blazor.Classes;

public class FilterItem(FilterItemTypes type, string name, string icon)
{
    public FilterItemTypes Type { get; set; } = type;
    public string Name { get; set; } = name;
    public string Icon { get; set; } = icon;
}