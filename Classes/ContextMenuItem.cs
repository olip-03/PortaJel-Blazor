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
        public static readonly BindableProperty itemSizeProperty = BindableProperty.Create(nameof(itemSize), typeof(double), typeof(ContextMenuItem), 0.0);
        public double itemSize
        {
            get => (double)GetValue(itemSizeProperty);
            set => SetValue(itemSizeProperty, value);
        }
        public static readonly BindableProperty cellSizeProperty = BindableProperty.Create(nameof(cellSize), typeof(double), typeof(ContextMenuItem), 0.0);
        public double cellSize
        {
            get => (double)GetValue(cellSizeProperty);
            set => SetValue(cellSizeProperty, value);
        }
        public static readonly BindableProperty isEnabledProperty = BindableProperty.Create(nameof(cellSize), typeof(bool), typeof(ContextMenuItem), false);
        public bool isEnabled
        {
            get => (bool)GetValue(isEnabledProperty);
            set => SetValue(isEnabledProperty, value);
        }

        public Action? action { get; set; }

        public ContextMenuItem(string setTaskName, string setTaskIcon, Action? setTask, double setIconSize = 35, double setCellSize = 50)
        {
            this.itemName = setTaskName;
            this.itemIcon = setTaskIcon;
            this.action = setTask;
            this.itemSize = setIconSize;
            this.cellSize = setCellSize;
            this.isEnabled = true;
        }
        public ContextMenuItem(string setTaskName, string setTaskIcon, Action? setTask)
        {
            this.itemName = setTaskName;
            this.itemIcon = setTaskIcon;
            this.action = setTask;
            this.isEnabled = true;
            this.itemSize = 35;
            this.cellSize = 50;
        }

        public ContextMenuItem(string setTaskName, MusicItemImage image, Action? setTask)
        {
            this.itemName = setTaskName;
            this.itemIcon = image.source;
            this.action = setTask;
            this.itemSize = 35;
            this.cellSize = 50;
            this.isEnabled = true;
        }

        public ContextMenuItem()
        {
            this.itemSize = 35;
            this.cellSize = 50;
            this.isEnabled = true;
        }
    }
}
