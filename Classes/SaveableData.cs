using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes
{
    public class SaveableData
    {
        public List<UserCredentials> connectionData = new();
        public bool firstLogin = false;
    }
}
