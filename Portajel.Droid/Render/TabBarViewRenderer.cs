using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Portajel.Droid.Render
{
    // TODO: This currently is not implemented bc it does not work. It would be nice to have the navbar behaving
    // the way I'd like but it's not neccesay for app functionality rn.
    internal class TabBarViewRenderer : ShellBottomNavViewAppearanceTracker
    {
        public TabBarViewRenderer(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem)
        {

        }
        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            bottomView.LayoutParameters.Height = 400;
            base.SetAppearance(bottomView, appearance);
        }
    }
}
