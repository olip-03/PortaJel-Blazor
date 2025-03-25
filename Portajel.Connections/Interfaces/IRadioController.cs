using Portajel.Connections.Data.Radio.Search;
using Portajel.Connections.Interfaces;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Portajel.Structures.Interfaces.Radio
{
    public interface IRadioController
    {
        public IRadioPlacesController Places { get; }
        public Task<RadioSearchResult?> SearchAsync(string query);
    }
}
