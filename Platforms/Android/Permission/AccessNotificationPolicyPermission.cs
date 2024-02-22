using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Platforms.Android.Permission
{
    internal class AccessNotificationPolicyPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
            (global::Android.Manifest.Permission.AccessNotificationPolicy, true),
            (global::Android.Manifest.Permission.PostNotifications, true)
        }.ToArray();
    }
}
