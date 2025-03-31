using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.Storage;
using Portajel.Connections;
using Portajel.Connections.Data;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Droid.Services;
using Portajel.Services;
using Portajel.Structures.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Android.Graphics.ColorSpace;

namespace PortaJel.Droid.Services
{
    public class DroidServiceController: Java.Lang.Object
    {
        public Portajel.Services.ServiceCollection AppServiceConnection { get; set; } = new();

        public DatabaseConnector Database => AppServiceConnection.Binder?.Database ?? throw new Exception("Binder is not set!");
        public ServerConnector Server => AppServiceConnection.Binder?.Server ?? throw new Exception("Binder is not set!");
        public DroidServiceController(IDbConnector db)
        {
            Intent mediaServiceIntent = new Intent(Platform.AppContext, typeof(DroidService));

            var appDataDirectory = Path.Combine(FileSystem.AppDataDirectory, "MediaData");
            string? mainDir = System.AppContext.BaseDirectory;
            var database = new DatabaseConnector(Path.Combine(mainDir, "portajeldb.sql"));
            var data = SaveHelper.LoadData(database, appDataDirectory);

            mediaServiceIntent.PutExtra("APICredentials", JsonSerializer.Serialize(data.Properties));
            Platform.AppContext.StartForegroundService(mediaServiceIntent);
            Platform.AppContext.BindService(mediaServiceIntent, this.AppServiceConnection, Bind.AutoCreate);
        }
    }
}
