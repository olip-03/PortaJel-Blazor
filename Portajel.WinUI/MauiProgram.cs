using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections;
using Portajel.Structures.Interfaces;
using Portajel.WinUI.Playback;
using CommunityToolkit.Maui;

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
            builder
                .UseSharedMauiApp()
                .UseMauiCommunityToolkit();
            return builder.Build();
        }
    }
}
