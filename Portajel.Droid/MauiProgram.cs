using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections;
using Portajel.Droid.Playback;
using Portajel.Structures.Interfaces;
using Microsoft.Maui.Hosting;
using Portajel.Pages.Settings.Debug;
using Portajel.Pages.Settings;
using Portajel.Droid.Services;
using PortaJel.Droid.Services;
using Portajel.Structures.Functional;
using Portajel.Services;

namespace Portajel.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string? mainDir = System.AppContext.BaseDirectory;
            if (mainDir == null) throw new SystemException("Could not find the main directory of the application.");
            var builder = MauiApp.CreateBuilder();

            builder.Services.AddSingleton(serviceProvider => {
                var database = new DatabaseConnector(Path.Combine(mainDir, "portajeldb.sql"));
                DroidServiceController droidServiceController = new DroidServiceController(database);
                return droidServiceController;
            });
            builder.Services.AddSingleton<IDbConnector, DroidDbConnector>(serviceProvider => {
                var service = serviceProvider.GetRequiredService<DroidServiceController>();
                DroidDbConnector droidServer = new DroidDbConnector(service);
                return droidServer;
            });
            // Add Foreground Service Connector
            builder.Services.AddSingleton<IServerConnector, DroidServerConnector>(serviceProvider => {
                var service = serviceProvider.GetRequiredService<DroidServiceController>();
                DroidServerConnector droidServer = new DroidServerConnector(service);
                return droidServer;
            });



            builder.Services.AddSingleton<IMediaController, MediaController>();
            builder.Services.AddSingleton<DroidServiceBinder>();
            builder.UseSharedMauiApp();
            return builder.Build();
        }
    }
}
