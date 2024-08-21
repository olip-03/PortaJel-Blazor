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
        public string SessionId { get; set; }
        public string SessionToken { get; set; }
        public UserCredentials(string serverAddress, string username, string password, string sessionId, string sessionToken) 
        {
            ServerAddress = serverAddress;
            UserName = username;
            Password = password;
            SessionId = sessionId;
            SessionToken = sessionToken;
        }
        public static UserCredentials Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    }
}
