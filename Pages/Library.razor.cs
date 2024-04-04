using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Pages
{
    public class LibraryCache
    {
        public List<BaseMusicItem> data { get; set; } = new();
        public string[] placeholderImages { get; set; } = new string[0];
        public int totalRecordCount { get; set; } = 0;

        public bool showGrid { get; set; } = false;

        public int loadLimit { get; set; } = 100;
        public int startFromIndex { get; set; } = 0;
        public int lastAmount { get; set; } = 0;

        public bool IsEmpty()
        {
            if (data.Count() == 0 &&
               placeholderImages.Length == 0 &&
               totalRecordCount == 0 &&
               startFromIndex == 0 &&
               lastAmount == 0)
            {
                return true;
            }
            return false;
        }
    }
}
