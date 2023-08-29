using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    public class ServerConnecter
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

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

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Address);

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("Username", username),
                new KeyValuePair<string, string>("Password", password)
            });
            
                var response = await httpClient.PostAsync("Users/AuthenticateByName", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
