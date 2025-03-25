using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.ViewModels.Settings
{
    public class ConnectionsPageViewModel
    {
        public ObservableCollection<IMediaServerConnector> Connectors { get; set; } = [];
    }
}
