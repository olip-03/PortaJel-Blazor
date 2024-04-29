using Android.Content;
using Android.Util;
using Android.OS;
namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    public class MediaServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string? TAG = typeof(MediaServiceConnection).FullName;
        public MediaServiceConnection()
        {
            IsConnected = false;
            Binder = null;
        }

        public bool IsConnected { get; private set; } = false;
        public MediaServiceBinder? Binder { get; private set; }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            Binder = service as MediaServiceBinder;
            IsConnected = this.Binder != null;

            if (name != null)
            {
                string message = "onServiceConnected - ";
                Log.Debug(TAG, $"OnServiceConnected {name.ClassName}");

                if (IsConnected)
                {
                    message = message + " bound to service " + name.ClassName;
                    // mainActivity.UpdateUiForBoundService();
                }
                else
                {
                    message = message + " not bound to service " + name.ClassName;
                    // mainActivity.UpdateUiForUnboundService();
                }

                Log.Info(TAG, message);
                // mainActivity.timestampMessageTextView.Text = message;
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            if (name != null)
            {
                Log.Debug(TAG, $"OnServiceDisconnected {name.ClassName}");
            }
            IsConnected = false;
            Binder = null;
            // mainActivity.UpdateUiForUnboundService();
        }
    }

}
