using System;

namespace PortaJel_Blazor.Classes.EventArgs;

public class CheckedItemEventArg(Guid itemId, bool isChecked) : System.EventArgs
{
    public Guid ItemId { get; set; } = itemId;
    public bool IsChecked { get; set; } = isChecked;
}