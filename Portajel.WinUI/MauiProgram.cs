using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections;
using Portajel.Structures.Interfaces;
using Portajel.WinUI.Playback;
using CommunityToolkit.Maui;
using Microsoft.Maui.Hosting;
using Portajel.Structures.Functional;

namespace Portajel.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string? mainDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if(mainDir == null) throw new SystemException("Could not find the main directory of the application.");
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<IMediaController, MediaController>();
            builder.Services.AddSingleton<IDbConnector>(serviceProvider =>
                new DatabaseConnector(Path.Combine(FileSystem.Current.AppDataDirectory, "portajeldb.sql")));
            builder.Services.AddSingleton<IServerConnector>(serviceProvider => {
                var database = serviceProvider.GetRequiredService<IDbConnector>();
                var appDataDirectory = Path.Combine(FileSystem.AppDataDirectory, "MediaData");
                return SaveHelper.LoadData(database, appDataDirectory);
            });
            builder
                .UseSharedMauiApp()
                .UseMauiCommunityToolkit();
            return builder.Build();
        }
    }
}
