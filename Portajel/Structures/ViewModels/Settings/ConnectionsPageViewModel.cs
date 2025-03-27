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
        public ObservableCollection<ServerBindingContext> Connectors { get; set; } = [];
    }

    public class ServerBindingContext
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string NavigationUrl { get; set; } = "";
        public ObservableCollection<SyncStatusInfoCollection> Syncs { get; set; } = new();
    }

    public class SyncStatusInfoCollection
    {
        public string Name { get; set; } = "";
        public SyncStatusInfo Status { get; set; } = new();
    }
}