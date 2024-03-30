namespace PortaJel_Blazor.Classes
{
    public class ContextMenuItem
    {
        public string itemName { get; set; } = String.Empty;
        public string itemIcon { get; set; } = String.Empty;
        public Task? action { get; set; }

        public ContextMenuItem(string setTaskName, string setTaskIcon, Task? setTask)
        {
            this.itemName = setTaskName;
            this.itemIcon = setTaskIcon;
            this.action = setTask;
        }
    }
}
