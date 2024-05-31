using Android.Content;
using Android.Util;
using Android.OS;
using PortaJel_Blazor.Data;
using System.Diagnostics;
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
        public MediaServiceConnection(BaseMusicItem? playingCollection, int fromIndex = 0)
        {
            PlayingCollection = playingCollection;
            PlayingCollecionFromIndex = fromIndex;
            IsConnected = false;
            Binder = null;
        }
        public MediaServiceConnection(Song addToQueue)
        {
            AddToQueue = addToQueue;
            IsConnected = false;
            Binder = null;
        }
        public BaseMusicItem? PlayingCollection { get; set; } = null;
        public int PlayingCollecionFromIndex { get; set; } = -1;
        public Song? AddToQueue { get; set; } = null;
        public bool IsConnected { get; private set; } = false;
        public MediaServiceBinder? Binder { get; private set; }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            MediaServiceBinder? serviceToBind = service as MediaServiceBinder;
            if(serviceToBind != null)
            {
                if (PlayingCollection != null && PlayingCollecionFromIndex >= 0)
                {
                    serviceToBind.PlayingCollection = PlayingCollection;
                    serviceToBind.PlayingCollecionFromIndex = PlayingCollecionFromIndex;
                }
                if (AddToQueue != null)
                {
                    serviceToBind.AddToQueue = AddToQueue;
                }
            }
            Binder = serviceToBind;
            IsConnected = this.Binder != null;

            if (name != null)
            {
                string message = "onServiceConnected - ";
                System.Diagnostics.Trace.WriteLine(TAG, $"OnServiceConnected {name.ClassName}");

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
