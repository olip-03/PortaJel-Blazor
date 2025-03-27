using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.OS;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using PortaJel.Droid.Services.MediaServer;
using Portajel.Services.Database;

namespace Portajel.Droid.Services
{
    public class ServiceBinder : Binder, IDisposable
    {
        private static DroidDatabaseConnector database = null!;
        private static DroidServerConnector serverConnector = null!;
        public ServiceBinder(DroidDatabaseConnector db, DroidServerConnector connector)
        {
            database = db;
            serverConnector = connector;
        }
    }
}
