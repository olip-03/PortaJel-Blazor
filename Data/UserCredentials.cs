using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Data
{
    public class UserCredentials
    {
        public string ServerAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public UserCredentials(string setServerAddress, string setUsername, string setPassword) 
        {
            ServerAddress = setServerAddress;
            UserName = setUsername;
            Password = setPassword;
        }
        public static UserCredentials Empty = new(string.Empty, string.Empty, string.Empty);
    }
}
