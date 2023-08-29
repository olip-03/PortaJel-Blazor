using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace PortaJel_Blazor.Classes
{
    // https://media.olisshittyserver.xyz/api-docs/swagger/index.html
    public class ServerConnecter
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private SdkClientSettings _sdkClientSettings;
        private ISystemClient _systemClient;
        private IUserClient _userClient;
        private IUserViewsClient _userViewsClient;

        HttpClient _httpClient;
        
        public ServerConnecter()
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();
        }

        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            try
            {
                HttpClient client = new HttpClient();

                using HttpResponseMessage response = await client.GetAsync(address);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Address = address;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            if (Address == null)
            {
                return false;
            }

            var host = Address;
            bool validServer = false;

            _sdkClientSettings.BaseUrl = host;
            _sdkClientSettings.DeviceName = "idk";
            _sdkClientSettings.DeviceId = "idk-either";
            _sdkClientSettings.ClientName = "PortaJel";
            _sdkClientSettings.ClientVersion = "0.01"; // baby steps

            _systemClient = new SystemClient(_sdkClientSettings, _httpClient);
            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync().ConfigureAwait(false);
                validServer = true;
                Console.WriteLine($"Connected to {host}");
                Console.WriteLine($"Server Name: {systemInfo.ServerName}");
                Console.WriteLine($"Server Version: {systemInfo.Version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var validUser = false;
            _userClient = new UserClient(_sdkClientSettings, _httpClient);
            UserDto userDto = null!;
            do
            {
                try
                {
                    Console.WriteLine($"Logging into {_sdkClientSettings.BaseUrl}");

                    // Authenticate user.
                    var authenticationResult = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                    {
                        Username = username,
                        Pw = password
                    }).ConfigureAwait(false);

                    _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
                    userDto = authenticationResult.User;
                    Console.WriteLine("Authentication success.");
                    Console.WriteLine($"Welcome to Jellyfin - {userDto.Name}");
                    validUser = true;
                }
                catch (UserException ex)
                {
                    await Console.Error.WriteLineAsync("Error authenticating.").ConfigureAwait(false);
                    await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
                }
            }
            while (!validUser);


            // Authentication failed
            return validUser;
        }
    }

}

