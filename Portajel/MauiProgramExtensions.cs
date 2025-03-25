using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Portajel.Connections;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Pages.Settings;
using Portajel.Structures.Functional;
using Portajel.Structures.Interfaces;
using Portajel.Structures.ViewModels.Settings;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Portajel
{
    public static class MauiProgramExtensions
    {
        public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
        {
            // Todo: Update minimum version requirements so this messaage goes away
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp(true)
                .RegisterViewModels()
                .RegisterViews()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            #if DEBUG
            builder.Logging.AddDebug();
            #endif
            return builder;
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<DatabaseConnector>(serviceProvider =>
                new DatabaseConnector(Path.Combine(FileSystem.Current.AppDataDirectory, "portajeldb.sql")));
            mauiAppBuilder.Services.AddSingleton<IDbConnector>(serviceProvider =>
                serviceProvider.GetRequiredService<DatabaseConnector>()); 
            mauiAppBuilder.Services.AddSingleton<IMediaServerConnector>(serviceProvider => {
                var database = serviceProvider.GetRequiredService<DatabaseConnector>();
                var appDataDirectory = Path.Combine(FileSystem.AppDataDirectory, "MediaData");
                return SaveHelper.LoadData(database, appDataDirectory);
            }); 
            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<ConnectionsPage>();
            // More views registered here.

            return mauiAppBuilder;
        }
    }
}
