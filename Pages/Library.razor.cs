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
        public bool isLoading { get; set; } = false;
        public Task? queuedTask { get; set; } = null;

        public bool showGrid { get; set; } = false;

        public int loadLimit { get; set; } = 100;
        public int loadRecordIndex { get; set; } = 25;
        public int itemsPerPage { get; set; } = 100;
        public int startFromIndex { get; set; } = 0;
        public int lastAmount { get; set; } = 0;
        public int pages { get; set; } = 0;

        public int lowerPages { get; set; } = 0;
        public int higherPages { get; set; } = 0;
    }
}
