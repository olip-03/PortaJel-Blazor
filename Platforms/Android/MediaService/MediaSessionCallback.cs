using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
   public class MediaSessionCallback : MediaSession.Callback
    {
        public Action? OnPlayImpl { get; set; }
        public Action<long>? OnSkipToQueueItemImpl { get; set; }
        public Action<long>? OnSeekToImpl { get; set; }
        public Action<string?, Bundle?>? OnPlayFromMediaIdImpl { get; set; }
        public Action? OnPauseImpl { get; set; }
        public Action? OnPlayPauseImpl { get; set; }
        public Action? OnStopImpl { get; set; }
        public Action? OnSkipToNextImpl { get; set; }
        public Action? OnSkipToPreviousImpl { get; set; }
        public Action<string?, Bundle?>? OnCustomActionImpl { get; set; }
        public Action<Intent?>? OnMediaButtonEventImpl { get; set; }

        public override void OnPlay()
        {
            if(OnPlayImpl != null)
            {
                OnPlayImpl();
            }
        }

        public override void OnSkipToQueueItem(long id)
        {
            if(OnSkipToQueueItemImpl != null)
            {
                OnSkipToQueueItemImpl(id);
            }
        }

        public override void OnSeekTo(long pos)
        {
            if (OnSeekToImpl != null)
            {
                OnSeekToImpl(pos);
            }
        }

        public override void OnPlayFromMediaId(string? mediaId, Bundle? extras)
        {
            if(OnPlayFromMediaIdImpl != null)
            {
                OnPlayFromMediaIdImpl(mediaId, extras);
            }
            
        }

        public override void OnPause()
        {
            if(OnPauseImpl != null)
            {
                OnPauseImpl();
            }
        }

        public override void OnStop()
        {
            if (OnStopImpl != null)
            {
                OnStopImpl();
            }
        }

        public override void OnSkipToNext()
        {
            if(OnSkipToNextImpl != null)
            {
                OnSkipToNextImpl();
            }
        }

        public override void OnSkipToPrevious()
        {
            if(OnSkipToPreviousImpl != null)
            {
                OnSkipToPreviousImpl();
            }
        }

        public override void OnCustomAction(string? action, Bundle? extras)
        {
            if(OnCustomActionImpl != null)
            {
                OnCustomActionImpl(action, extras);
            }
        }

        /// <summary>
        /// Override to handle media button events.
        /// The double tap of KEYCODE_MEDIA_PLAY_PAUSE or KEYCODE_HEADSETHOOK will call the onSkipToNext by default. If the current SDK level is 27 or higher, the default double tap handling is done by framework so this method would do nothing for it.
        /// </summary>
        /// <param name="mediaButtonIntent"></param>
        /// <returns>True if the event was handled, false otherwise.</returns>
        public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
        {
            if(OnMediaButtonEventImpl != null)
            {
                OnMediaButtonEventImpl(mediaButtonIntent);
                return true;
            }
            return false;
        }
    }
}
