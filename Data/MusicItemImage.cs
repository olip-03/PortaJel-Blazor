using Blurhash;
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
        public int soureResolution { get; set; } = 50;
        public string sourceAtResolution { get
            {
                return SourceAtResolution(soureResolution);
            }
            private set { }
        }
        public MusicItemImageType musicItemImageType { get; set; } = MusicItemImageType.url;
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

        /// <summary>
        /// Appends the URL with information that'll request it at a different size. Useful for web optimization when we dont need a full res image.
        /// </summary>
        /// <param name="px">The reqeusted resolution in pixels</param>
        /// <returns>Image at the requested resolution, by the 'px' variable.</returns>
        public string SourceAtResolution(int px)
        {
            if(musicItemImageType == MusicItemImageType.url)
            {
                string toReturn = source + $"&fillHeight={px}&fillWidth={px}&quality=96";
                return toReturn;
            }
            return source;
        }
    }
}
