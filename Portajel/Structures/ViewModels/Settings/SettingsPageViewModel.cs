using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.ViewModels.Settings
{
    public class SettingsPageViewModel
    {
        public ObservableCollection<ListItem> ListItems { get; set; } = new();
    }
}
