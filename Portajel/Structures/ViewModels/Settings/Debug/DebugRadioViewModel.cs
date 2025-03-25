using Portajel.Connections.Data.Radio.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portajel.Structures.ViewModels.Settings.Debug
{
    public class DebugRadioViewModel
    {
        public ObservableCollection<RadioSearchHit> SearchResults { get; set; } = new();
    }
}
