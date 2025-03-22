namespace Portajel.Connections.Data
{
    public class UserCredentials
    {
        public string ServerAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string SessionToken { get; set; }
        public UserCredentials(string serverAddress, string username, string userid, string password, string sessionId, string sessionToken) 
        {
            ServerAddress = serverAddress;
            UserName = username;
            UserId = userid;
            Password = password;
            SessionId = sessionId;
            SessionToken = sessionToken;
        }
        public static UserCredentials Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    }
}