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
        public ServerConnecter(string deviceName, string deviceId, string clientName, string clientVersion)
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();

            _sdkClientSettings.DeviceName = deviceName;
            _sdkClientSettings.DeviceId = deviceId;
            _sdkClientSettings.ClientName = clientName;
            _sdkClientSettings.ClientVersion = clientVersion;
        }

        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            _sdkClientSettings.BaseUrl = address;

            _systemClient = new SystemClient(_sdkClientSettings, _httpClient);
            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            if (_systemClient == null)
            {
                return false;
            }

            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }

            var validUser = false;
            _userClient = new UserClient(_sdkClientSettings, _httpClient);
            UserDto userDto = null!;
            try
            {
                // Authenticate user.
                var authenticationResult = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                {
                    Username = username,
                    Pw = password
                }).ConfigureAwait(false);

                _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
                userDto = authenticationResult.User;

                validUser = true;
            }
            catch (Exception)
            {
                return false;
            }

            return validUser;
        }
    }

}

