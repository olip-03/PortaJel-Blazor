using System.Drawing;
using Android.App;
using Android.Graphics;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.UI;
using Java.Interop;
using Java.Lang;
using PortaJel_Blazor.Classes.Data;
using Bitmap = System.Drawing.Bitmap;

namespace PortaJel_Blazor.Platforms.Android.MediaService
{
    [Obsolete]
    internal class MediaServiceDescriptionAdaptor : Java.Lang.Object,  PlayerNotificationManager.IMediaDescriptionAdapter
    {
        public nint Handle => throw new NotImplementedException();

        public int JniIdentityHashCode => throw new NotImplementedException();

        public JniObjectReference PeerReference => throw new NotImplementedException();

        public JniPeerMembers JniPeerMembers => throw new NotImplementedException();

        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        private List<Song> songs = new();
        private int songIndex = 0;

        public MediaServiceDescriptionAdaptor()
        {
            
        }

        public void UpdatePlayerData(Song[]? setSongs = null, string[]? setBlurhash = null, int setIndex = -1)
        {
            if(setSongs != null && setSongs.Count() > 0)
            {
                songs = setSongs.ToList();
            }

            if(setIndex >= 0)
            {
                songIndex = 0;
            }
        }

        public PendingIntent? CreateCurrentContentIntent(IPlayer? player)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public void Disposed()
        {
            // throw new NotImplementedException();
        }

        public void DisposeUnlessReferenced()
        {
            // throw new NotImplementedException();
        }

        public void Finalized()
        {
            // throw new NotImplementedException();
        }

        public ICharSequence? GetCurrentContentTextFormatted(IPlayer? player)
        {
            throw new NotImplementedException();
        }

        public ICharSequence? GetCurrentContentTitleFormatted(IPlayer? player)
        {
            throw new NotImplementedException();
        }

        public global::Android.Graphics.Bitmap GetCurrentLargeIcon(IPlayer? player, PlayerNotificationManager.BitmapCallback? callback)
        {
            return null;
        }

        public void SetJniIdentityHashCode(int value)
        {
            // throw new NotImplementedException();
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
            // throw new NotImplementedException();
        }

        public void SetPeerReference(JniObjectReference reference)
        {
            // throw new NotImplementedException();
        }

        public void UnregisterFromRuntime()
        {
            // throw new NotImplementedException();
        }
    }
}
