using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class Artist : BaseMusicItem
    {
        public string description { get; set; } = String.Empty;
        public string imgSrc { get; set; } = String.Empty;
        public string backgroundImgSrc { get; set; } = String.Empty;
        public string logoImgSrc { get; set; } = String.Empty;
        public Album[] artistAlbums { get; set; } = new Album[0];

        public static Artist Empty = new Artist(); 

        public string imageAtResolution(int px)
        {
            return imgSrc += $"&fillHeight={px}&fillWidth={px}&quality=96";
        }
        public string backgroundImgAtResolution(int px)
        {
            return backgroundImgSrc += $"&fillHeight={px}&fillWidth={px}&quality=96";
        }
    }
}
