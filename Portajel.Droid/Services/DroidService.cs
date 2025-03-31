using Android.Content;
using Microsoft.Maui.Controls;
using Portajel.Connections.Services.Database;
using PortaJel.Droid.Services;
using Portajel.Structures.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Portajel.Connections.Interfaces;
using Portajel.Connections;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Portajel.Services;

namespace Portajel.Droid.Services
{
    [Service(Name = "PortaJel.Droid.ServiceController", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class DroidService : Service
    {
        public IBinder Binder { get; set; } = default!;
        public DatabaseConnector database { get; private set; } = null!;
        public ServerConnector serverConnector { get; private set; } = null!;
        public DroidService()
        {
            string? mainDir = System.AppContext.BaseDirectory;
            database = new DatabaseConnector(Path.Combine(mainDir, "portajeldb.sql"));
        }
        public override IBinder? OnBind(Intent? intent)
        {
            var appDataDirectory = Path.Combine(FileSystem.AppDataDirectory, "MediaData");
            serverConnector = SaveHelper.LoadData(database, appDataDirectory);
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            this.Binder = new DroidServiceBinder(this);
            return this.Binder;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                string? value = intent.GetStringExtra("APICredentials");
                if (value != null)
                {
                    var appDataDirectory = Path.Combine(FileSystem.AppDataDirectory, "MediaData");
                    ServerConnectorSettings settings = new(value, database, appDataDirectory);
                    serverConnector = settings.ServerConnector;
                }
            }
            return base.OnStartCommand(intent, flags, startId);
        }
    }
}
