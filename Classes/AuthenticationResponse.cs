namespace PortaJel_Blazor.Classes
{
    public class AuthenticationResponse
    {
        // Indicates if the authentication request was successful
        public bool IsSuccess { get; set; }

        // The token received upon successful authentication (e.g., JWT)
        public string Token { get; set; }

        // The expiration time of the token
        public DateTime TokenExpiry { get; set; }

        // An optional message providing additional information about the authentication result
        public string Message { get; set; }

        // The user's ID, if applicable
        public string UserId { get; set; }

        // The user's username or email, if applicable
        public string Username { get; set; }

        // Any roles associated with the authenticated user
        public List<string> Roles { get; set; } = new List<string>();

        // Additional data related to the response (e.g., user profile info)
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();

        public static AuthenticationResponse Ok()
        {
            return new AuthenticationResponse
            {
                IsSuccess = true,
                Token = string.Empty,
                TokenExpiry = DateTime.MaxValue,
                Message = "OK",
                UserId = "Anonymous",
                Username = "Anonymous",
                Roles = new List<string> { "Anonymous" },
                AdditionalData = new Dictionary<string, string> { { "AccessLevel", "Anonymous" } }
            };
        }
        public static AuthenticationResponse Unauthorized(string message = "Unauthorized")
        {
            return new AuthenticationResponse
            {
                IsSuccess = false,
                Token = string.Empty,
                TokenExpiry = DateTime.MaxValue,
                Message = message,
                UserId = "Anonymous",
                Username = "Anonymous",
                Roles = new List<string> { "Anonymous" },
                AdditionalData = new Dictionary<string, string> { { "AccessLevel", "Anonymous" } }
            };
        }
        public static AuthenticationResponse Unneccesary()
        {
            return new AuthenticationResponse
            {
                IsSuccess = true,
                Token = string.Empty,
                TokenExpiry = DateTime.MaxValue,
                Message = "This connection does not need to be authenticated, anonymous access is permitted by default",
                UserId = "Anonymous",
                Username = "Anonymous",
                Roles = new List<string> { "Anonymous" },
                AdditionalData = new Dictionary<string, string> { { "AccessLevel", "Anonymous" } }
            };
        }
    }

}
