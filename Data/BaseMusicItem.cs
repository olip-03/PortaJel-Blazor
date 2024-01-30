using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class BaseMusicItem
    {
        public Guid id { get; set; } = Guid.Empty;
        public string name { get; set; } = String.Empty;
        public MusicItemImage image { get; set; } = MusicItemImage.Empty;
    }
}
