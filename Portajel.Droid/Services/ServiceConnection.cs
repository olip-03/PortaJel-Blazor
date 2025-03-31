using Android.Content;
using Android.OS;
using Android.Util;
using Jellyfin.Sdk.Generated.Models;
using Portajel.Connections.Data;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services;
using Portajel.Droid.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Services
{
    public class ServiceCollection : Java.Lang.Object, IServiceConnection
    {
        static readonly string? TAG = typeof(ServiceCollection).FullName;
        public bool IsConnected { get; private set; } = false;
        public DroidServiceBinder? Binder { get; private set; }
        public ServiceCollection()
        {
            IsConnected = false;
            Binder = null;
        }
        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            DroidServiceBinder? serviceToBind = service as DroidServiceBinder;

            Binder = serviceToBind;
            IsConnected = Binder != null;

            if (name != null)
            {
                string message = "onServiceConnected - ";
                System.Diagnostics.Trace.WriteLine(TAG, $"OnServiceConnected {name.ClassName}");
                if (IsConnected)
                {
                    message = message + " bound to service " + name.ClassName;
                }
                else
                {
                    message = message + " not bound to service " + name.ClassName;
                }
                Log.Info(TAG, message);
            }
        }
        public void OnServiceDisconnected(ComponentName? name)
        {
            if (name != null)
            {
                Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
            }
            if (IsConnected && Binder != null)
            {
                Binder.Destroy();
            }
            IsConnected = false;
            Binder = null;
        }
    }
}
