using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class GoBack
    {
        private static IJSRuntime JSRuntime { get; set; }

        public GoBack(IJSRuntime jSRuntime)
        {
            GoBack.JSRuntime = jSRuntime;
        }

        public static async Task GoBackInTime()
        {
            //Microsoft.Maui.Platform;
            if (GoBack.JSRuntime != null)
            {
                await GoBack.JSRuntime.InvokeVoidAsync("goBack");
            }
        }
    }
}
