using PortaJel_Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class SaveableData
    {
        public List<UserCredentials> connectionData = new();
        public bool firstLogin = false;
    }
}
