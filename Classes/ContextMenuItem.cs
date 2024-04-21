using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Controls.Shapes;
using PortaJel_Blazor.Classes;
using PortaJel_Blazor.Data;
using System.Collections.ObjectModel;

namespace PortaJel_Blazor.Classes
{
    public class ContextMenuItem : BindableObject
    {
        public static readonly BindableProperty itemNameProperty = BindableProperty.Create(nameof(itemName), typeof(string), typeof(ContextMenuItem), string.Empty);
        public string itemName
        {
            get => (string)GetValue(itemNameProperty);
            set => SetValue(itemNameProperty, value);
        }
        public static readonly BindableProperty itemIconProperty = BindableProperty.Create(nameof(itemIcon), typeof(string), typeof(ContextMenuItem), string.Empty);
        public string itemIcon {
            get => (string)GetValue(itemIconProperty);
            set => SetValue(itemIconProperty, value);
        } 
        public Task? action { get; set; }

        public ContextMenuItem(string setTaskName, string setTaskIcon, Task? setTask)
        {
            this.itemName = setTaskName;
            this.itemIcon = setTaskIcon;
            this.action = setTask;
        }

        public ContextMenuItem()
        {

        }
    }
}
