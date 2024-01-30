using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class MusicItemImage
    {
        // Variables
        public string source { get; set; } = String.Empty;
        public string blurHash { get; set; } = String.Empty;
        public MusicItemImageType musicItemImageType { get; set; } = MusicItemImageType.onDisk;
        public enum MusicItemImageType
        {
            onDisk,
            url,
            base64,
        }
        ///<summary>
        ///A read-only instance of the Data.MusicItemImage structure with default values.
        ///</summary>
        public static readonly MusicItemImage Empty = new();
    }
}
