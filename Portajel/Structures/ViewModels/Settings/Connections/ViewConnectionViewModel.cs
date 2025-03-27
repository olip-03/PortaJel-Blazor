using Portajel.Connections.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.ViewModels.Settings.Connections
{
    public class ViewConnectionViewModel
    {
        public ObservableCollection<ConnectorProperty> ConnectionItems { get; set; } = new();
    }
}
