using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class StyleSettings
    {
        /// <summary>
        /// Determins if items shown in lists display three dots to open the context menu, instead of a heart to favourite the item.
        /// </summary>
        public bool listItemsContextMenu { get; set; } = true;
    }
}
