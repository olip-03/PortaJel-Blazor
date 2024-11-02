using Microsoft.JSInterop;

namespace PortaJel_Blazor.Classes;

public class JsInteropClasses2 : IDisposable
{
    private readonly IJSRuntime js;

    public JsInteropClasses2(IJSRuntime js)
    {
        this.js = js;
    }

    public async ValueTask<string> CloseElement()
    {
        return await js.InvokeAsync<string>("dragElement", "close");
    }
    public async ValueTask<string> OpenElement()
    {
        return await js.InvokeAsync<string>("dragElement", "open");
    }
    public async ValueTask<string> OnLabelDrag()
    {
        return await js.InvokeAsync<string>("dragElement", "music-player");
    }

    public void Dispose()
    {
    }
}