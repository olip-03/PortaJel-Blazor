using System.Text;

namespace PortaJel_Blazor.Classes;

public static class Base64Url
{
    public static string Encode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(text);
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2")); // Convert byte to hex (2 chars)

        return sb.ToString();
    }

    public static string Decode(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return string.Empty;

        if (hex.Length % 2 != 0)
            throw new ArgumentException("Invalid hex string length", nameof(hex));

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

        return Encoding.UTF8.GetString(bytes);
    }
}