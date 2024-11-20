namespace PortaJel_Blazor.Classes;

using System;
using System.Security.Cryptography;
using System.Text;

public static class GuidHelper
{
    public static Guid GenerateNewGuidFromHash(Guid inputGuid, string inputString)
    {
        // Combine the GUID and string into a single byte array
        byte[] guidBytes = inputGuid.ToByteArray();
        byte[] stringBytes = Encoding.UTF8.GetBytes(inputString);
        
        byte[] combinedBytes = new byte[guidBytes.Length + stringBytes.Length];
        Buffer.BlockCopy(guidBytes, 0, combinedBytes, 0, guidBytes.Length);
        Buffer.BlockCopy(stringBytes, 0, combinedBytes, guidBytes.Length, stringBytes.Length);

        // Compute a hash using SHA256
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(combinedBytes);

            // Use the first 16 bytes of the hash to create a new GUID
            byte[] guidHashBytes = new byte[16];
            Array.Copy(hashBytes, guidHashBytes, 16);

            // Return the new GUID
            return new Guid(guidHashBytes);
        }
    }
}
