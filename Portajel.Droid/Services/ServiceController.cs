using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Portajel.Connections;
using Portajel.Connections.Services.Database;
using Portajel.Droid.Services;
using Portajel.Services.Database;
using PortaJel.Droid.Services.MediaServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel.Droid.Services
{
    [Service(Name = "PortaJel.Droid.ServiceController", IsolatedProcess = true, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class ServiceController : Service
    {
        public IBinder Binder { get; set; } = default!;

        public override IBinder? OnBind(Intent? intent)
        {
            this.Binder = new ServiceBinder();
            Context context = Microsoft.Maui.ApplicationModel.Platform.AppContext;
            return this.Binder;
        }
        private void StartForeground()
        {

        }
    }
}
