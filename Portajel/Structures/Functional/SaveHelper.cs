using Microsoft.Maui.Controls;
using Portajel.Connections;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Services;
using PortaJel_Blazor.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.Functional
{
    public static class SaveHelper
    {
        private static string model = DeviceInfo.Current.Model;
        private static string manufacturer = DeviceInfo.Current.Manufacturer;
        private static string deviceName = DeviceInfo.Current.Name;
        public static ServerConnector LoadData(IDbConnector database, string appDataDirectory)
        {
            Task<string?> r = SecureStorage.Default.GetAsync(GuidHelper.GetDeviceHash(model, manufacturer, deviceName));
            r.Wait();
            string? result = r.Result;

            if (result == null)
            {
                return new ServerConnector();
            }

            ServerConnectorSettings settings = new(result, database, appDataDirectory);
            return settings.ServerConnector;
        }
        public static async Task<bool> SaveData(IServerConnector server)
        {
            try
            {
                string toSave = System.Text.Json.JsonSerializer.Serialize(server.Properties);
                await SecureStorage.Default.SetAsync(GuidHelper.GetDeviceHash(model, manufacturer, deviceName), toSave);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"SaveData(): {e.Message}");
                return false;
            }
            return true;
        }
    }
}
