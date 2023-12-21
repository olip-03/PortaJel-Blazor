using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class ContextMenuItem
    {
        public string itemName { get; set; } = String.Empty;
        public string itemIcon { get; set; } = String.Empty;
        public Task? job { get; set; }

        public ContextMenuItem(string itemName, string itemIcon, Task? job)
        {
            this.itemName = itemName;
            this.itemIcon = itemIcon;
            this.job = job;
        }
    }
}
