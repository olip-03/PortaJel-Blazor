using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections;
using Portajel.Droid.Playback;
using Portajel.Structures.Interfaces;

namespace Portajel.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string? mainDir = System.AppContext.BaseDirectory;
            if (mainDir == null) throw new SystemException("Could not find the main directory of the application.");
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<IDbConnector>(serviceProvider => new DatabaseConnector(Path.Combine(mainDir, "portajeldb.sql")));
            builder.Services.AddSingleton<IMediaServerConnector, ServerConnector>();
            builder.Services.AddSingleton<IMediaController, MediaController>();
            builder.UseSharedMauiApp();
            return builder.Build();
        }
    }
}
