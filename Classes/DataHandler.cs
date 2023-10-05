using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    /// <summary>
    /// Main class for handling data from many servers, as well as cached data and whatever
    /// </summary>
    public class DataHandler
    {
        private List<ServerConnecter> connecters;
        public DataHandler(List<ServerConnecter> setConnectors) 
        {
            connecters = setConnectors;
        }
    }
}
